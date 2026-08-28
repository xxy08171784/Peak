using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.Acts;

namespace peak.Patches;

[HarmonyPatch]
public static class Act4MapPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.Constructor(typeof(StandardActMap), new[]
        {
            typeof(Rng), typeof(ActModel), typeof(bool),
            typeof(bool), typeof(bool),
            typeof(MapPointTypeCounts), typeof(bool),
        });
    }

    [HarmonyPostfix]
    static void Postfix(StandardActMap __instance, ActModel actModel)
    {
        if (actModel is Act4)
        {
            __instance.StartingMapPoint.PointType = MapPointType.Shop;

            // Grid 是 ActMap 基类的 protected 属性，必须用 GetProperty 访问
            var gridProp = typeof(ActMap).GetProperty("Grid",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (gridProp?.GetValue(__instance) is MapPoint[,] grid)
            {
                // 把所有战斗/宝箱节点→Shop，无战斗无宝箱
                // TreasureRoom 构造函数只接受 actIndex 0~2，Act4(Index=3)会崩
                for (int col = 0; col < grid.GetLength(0); col++)
                {
                    for (int row = 0; row < grid.GetLength(1); row++)
                    {
                        var pt = grid[col, row];
                        if (pt == null) continue;
                        if (pt.PointType == MapPointType.Monster ||
                            pt.PointType == MapPointType.Elite ||
                            pt.PointType == MapPointType.Treasure)
                            pt.PointType = MapPointType.Shop;
                    }
                }
            }
        }
    }
}

/// <summary>
/// 在 StandardActMap 构造后，把 _pointTypeCounts 里的精英和商店清零。
/// 这样 AssignPointTypes + PruneAndRepair 就不会硬塞战斗节点。
/// 配合 Postfix 的 Grid 遍历双重保障。
/// </summary>
[HarmonyPatch]
public static class Act4MapCountsPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.Constructor(typeof(StandardActMap), new[]
        {
            typeof(Rng), typeof(ActModel), typeof(bool),
            typeof(bool), typeof(bool),
            typeof(MapPointTypeCounts), typeof(bool),
        });
    }

    [HarmonyPostfix]
    static void Postfix(StandardActMap __instance, ActModel actModel)
    {
        if (actModel is Act4)
        {
            // 用反射取 StandardActMap 的私有字段 _pointTypeCounts
            var countsField = typeof(StandardActMap).GetField("_pointTypeCounts",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (countsField?.GetValue(__instance) is MapPointTypeCounts counts)
            {
                // NumOfElites 和 NumOfShops 是 init-only 属性，
                // 反射写 backing field 清零
                var eliteField = counts.GetType().GetField("<NumOfElites>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                eliteField?.SetValue(counts, 0);

                var shopField = counts.GetType().GetField("<NumOfShops>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                shopField?.SetValue(counts, 0);
            }
        }
    }
}

[HarmonyPatch]
public static class Act4MapFixPatch
{
    private static List<MapCoord> _backupVisited = new();

    [HarmonyPatch(typeof(NMapScreen), "SetMap")]
    [HarmonyPostfix]
    static void SetMap_Postfix(NMapScreen __instance, ActMap map)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not IRunState runState || runState.Act is not Act4)
            return;

        var bpnField = typeof(NMapScreen).GetField("_bossPointNode",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (bpnField?.GetValue(__instance) is not NMapPoint bossNode)
            return;

        var distXField = typeof(NMapScreen).GetField("_distX",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var distYField = typeof(NMapScreen).GetField("_distY",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (distXField == null || distYField == null) return;

        float distX = (float)distXField.GetValue(__instance);
        float distY = (float)distYField.GetValue(__instance);
        int lastRow = map.GetRowCount() - 1;

        // Boss 水平位置：用最后一行（篝火行）所有可见节点的平均 X，让 Boss 水平居中于
        // 地图底部行。固定 -200 在稀疏地图上会落到最左侧、只露出一半。
        // _mapPointDictionary 里的节点与 Boss 处于同一坐标空间（都是 _points 的子节点）。
        float newBX = -200f;
        var dictField = typeof(NMapScreen).GetField("_mapPointDictionary",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (dictField?.GetValue(__instance) is Dictionary<MapCoord, NMapPoint> dict)
        {
            var rowXs = dict.Values
                .Where(n => n.Point != null && n.Point.coord.row == lastRow)
                .Select(n => n.Position.X)
                .ToList();
            if (rowXs.Count > 0)
                newBX = (float)rowXs.Average();
        }

        // y 放在最后一行 grid 下方 0.5 个行距处，让 Boss 可见且不与其他节点重叠
        float lastGridRowY = (float)lastRow * (-distY) + 740f;
        float newBY = lastGridRowY + distY * 0.5f;

        // X 向左偏移 100px，让 Boss 图标居中偏左
        bossNode.Position = new Vector2(newBX - 100f, newBY);
        GD.Print($"[Act4Fix] Boss pos=({newBX},{newBY}) lastRow={lastRow}");
    }

    [HarmonyPatch(typeof(NMapScreen), "Open")]
    [HarmonyPrefix]
    static void Open_Prefix(NMapScreen __instance)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not RunState runState || runState.Act is not Act4)
            return;

        var visitedField = typeof(RunState).GetField("_visitedMapCoords",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (visitedField?.GetValue(runState) is not List<MapCoord> list)
            return;

        if (list.Count > 0)
        {
            _backupVisited = new List<MapCoord>(list);
        }
        else if (_backupVisited.Count > 0)
        {
            // 只恢复当前地图上仍然有效的坐标；_backupVisited 是静态字段，
            // 跨周目/地图重新生成后旧坐标可能已失效，必须过滤，否则会误标 Traveled/Boss。
            var valid = _backupVisited
                .Where(c => runState.Map != null && runState.Map.HasPoint(c))
                .ToList();
            if (valid.Count > 0)
            {
                list.Clear();
                list.AddRange(valid);
                GD.Print($"[Act4Fix] Restored {valid.Count} visited coords");
            }
            else
            {
                _backupVisited.Clear();
                GD.Print("[Act4Fix] Backup visited coords stale, dropped");
            }
        }
    }

    /// <summary>
    /// 每次真正走到一个新节点时同步备份 visited。
    /// 篝火期间 visited 会被（trainer 等外部路径）清空，而 Open 时恢复用的备份
    /// 来自上一次 Open（缺了当前篝火坐标），导致篝火被当成未访问、可再次点击。
    /// 在 AddVisitedMapCoord 处实时刷新备份，恢复时就能包含玩家当前位置。
    /// </summary>
    [HarmonyPatch(typeof(RunState), "AddVisitedMapCoord")]
    [HarmonyPostfix]
    static void AddVisitedMapCoord_Postfix(RunState __instance, bool __result)
    {
        if (!__result) return;
        if (__instance.Act is not Act4) return;

        _backupVisited = new List<MapCoord>(__instance.VisitedMapCoords);
        GD.Print($"[Act4Fix] Backup visited updated to {_backupVisited.Count} coords");
    }

    [HarmonyPatch(typeof(NMapScreen), "Open")]
    [HarmonyPostfix]
    static void Open_Postfix(NMapScreen __instance)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not RunState runState || runState.Act is not Act4)
            return;

        var hasPlayedField = typeof(NMapScreen).GetField("_hasPlayedAnimation",
            BindingFlags.Instance | BindingFlags.NonPublic);
        hasPlayedField?.SetValue(__instance, true);

        // Fix camera position to the last valid visited row
        int lastRow = 0;
        if (runState.VisitedMapCoords.Any())
            lastRow = runState.VisitedMapCoords.Last().row;

        var dragField = typeof(NMapScreen).GetField("_targetDragPos",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var dragPos = dragField?.GetValue(__instance) is Vector2 dv ? dv : Vector2.Zero;

        var distYField = typeof(NMapScreen).GetField("_distY",
            BindingFlags.Instance | BindingFlags.NonPublic);
        float distY = distYField != null ? (float)distYField.GetValue(__instance) : 387.5f;
        float expectedY = -600f + (float)lastRow * distY;

        if (dragPos.Y < expectedY - 50f || dragPos.Y > expectedY + 50f)
        {
            float targetY = -600f + (float)lastRow * distY;
            dragField?.SetValue(__instance, new Vector2(0f, targetY));

            // 开场动画播放中时容器位置由 tween 接管，只更新目标位置；
            // 动画结束后 _Process 会平滑滚动到 targetY。
            var animTweenField = typeof(NMapScreen).GetField("_actAnimTween",
                BindingFlags.Instance | BindingFlags.NonPublic);
            bool animRunning = animTweenField?.GetValue(__instance) is Tween at && at.IsRunning();

            var containerField = typeof(NMapScreen).GetField("_mapContainer",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (!animRunning && containerField?.GetValue(__instance) is Control mc)
            {
                var p = mc.Position;
                p.Y = targetY;
                mc.Position = p;
            }
            GD.Print($"[Act4Fix] Camera fixed to row {lastRow} (y={targetY}, anim={animRunning})");
        }

// FIX: 重新同步 MapSelectionSynchronizer 状态。
        // 从篝火出来后，玩家实际仍在篝火坐标，但同步器可能停留在 (null, actIndex)。
        // 恢复 _acceptingVotesFromSource = (currentCoord, actIndex)。
        // 不预填 _votes，让投票从 null 开始等玩家自己点击，否则预填"当前房间"的投票
        // 会导致主机随机选票时可能选到"不动"的票，多人无法前进。
        try
        {
            var syncField = typeof(RunManager).GetProperty("MapSelectionSynchronizer",
                BindingFlags.Instance | BindingFlags.Public);
            var sync = syncField?.GetValue(RunManager.Instance);
            if (sync == null) return;

            var syncType = sync.GetType();
            var currentCoord = runState.CurrentMapCoord;
            if (currentCoord == null) return;

            var acceptField = syncType.GetField("_acceptingVotesFromSource",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (acceptField != null)
            {
                var newLocation = new MapLocation(currentCoord, runState.CurrentActIndex);
                acceptField.SetValue(sync, newLocation);
                GD.Print($"[Act4Fix] Sync source fixed to {currentCoord}");
            }
        }
        catch (Exception ex)
        {
            GD.Print($"[Act4Fix] Sync fix failed: {ex}");
        }
    }

    /// <summary>
    /// 阻止玩家点击已访问过的地图节点（如已用过的篝火）。
    /// 即使节点因状态同步问题仍显示为可点击，投票也会被拒绝，杜绝重复进入。
    /// 检查 visited / 玩家当前位置 / Traveled 状态三层兜底。
    /// </summary>
    [HarmonyPatch(typeof(NMapScreen), "OnMapPointSelectedLocally")]
    [HarmonyPrefix]
    static bool OnMapPointSelectedLocally_Prefix(NMapScreen __instance, NMapPoint point)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not RunState runState || runState.Act is not Act4)
            return true;

        if (point?.Point == null)
            return true;

        var coord = point.Point.coord;
        bool isVisited = runState.VisitedMapCoords.Contains(coord);
        bool isCurrent = runState.CurrentMapCoord == coord;
        if (isVisited || isCurrent || point.State == MapPointState.Traveled)
        {
            GD.Print($"[Act4Fix] Blocked click on {coord} (visited={isVisited}, cur={isCurrent}, state={point.State})");
            return false;
        }
        return true;
    }
}