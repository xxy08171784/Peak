using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.Acts;

namespace peak.Patches;

/// <summary>
/// 第4幕地图补丁。
/// Postfix StandardActMap 构造函数：把起始点从 Ancient(涅奥) 改为 Shop。
/// Act4 没有 Ancient 事件，起点必须是商店才能正常进入（绕过涅奥卡死）。
/// 必须写出构造器全部 7 个参数类型（默认参数编译后仍是完整签名），否则 Harmony 无法精确匹配。
/// </summary>
[HarmonyPatch]
public static class Act4MapPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.Constructor(typeof(StandardActMap), new[]
        {
            typeof(Rng),
            typeof(ActModel),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(MapPointTypeCounts),
            typeof(bool),
        });
    }

    [HarmonyPostfix]
    static void Postfix(StandardActMap __instance, ActModel actModel)
    {
        if (actModel is Act4)
        {
            __instance.StartingMapPoint.PointType = MapPointType.Shop;
        }
    }
}

/// <summary>
/// Act4 地图整体修复补丁：
/// 1. Boss 节点重定位（代替硬编码 -1980）
/// 2. 篝火后 _visitedMapCoords 被意外清空时保证地图相机位置 + Boss 可点击
/// </summary>
[HarmonyPatch]
public static class Act4MapFixPatch
{
    // ──────────────────────────────────────────
    // 1. SetMap → Boss 重定位
    // ──────────────────────────────────────────
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

        // Boss X 坐标：按地图宽度居中，不用 -200
        var distXField = typeof(NMapScreen).GetField("_distX",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var distYField = typeof(NMapScreen).GetField("_distY",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (distXField == null || distYField == null) return;

        float distX = (float)distXField.GetValue(__instance);
        float distY = (float)distYField.GetValue(__instance);
        int lastRow = map.GetRowCount() - 1;
        int bossCol = map.BossMapPoint.coord.col;

        // X: 按列居中（StandardActMap Boss 在 col=3，-500 + 3*distX）
        float newBX = -500f + (float)bossCol * distX;
        // Y: 最后 Grid 行下方 1.0 行间距（确保刚好在可见范围内）
        float lastGridRowY = (float)(lastRow) * (-distY) + 740f;
        float newBY = lastGridRowY + distY * 1.0f;

        bossNode.Position = new Vector2(newBX, newBY);

        GD.Print($"[Act4Fix] Boss pos=({newBX},{newBY}) lastRow={lastRow} distY={distY}");
    }

    // ──────────────────────────────────────────
    // 2. Open → _visitedMapCoords 为空时用 ActFloor 修正镜头
    // ──────────────────────────────────────────
    [HarmonyPatch(typeof(NMapScreen), "Open")]
    [HarmonyPostfix]
    static void Open_Postfix(NMapScreen __instance)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not RunState runState || runState.Act is not Act4)
            return;

        // 有 visited 数据则信任原版
        if (runState.VisitedMapCoords.Any())
            return;

        // visited 被清空 → 用 ActFloor 估算当前行
        var distYField = typeof(NMapScreen).GetField("_distY",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var targetField = typeof(NMapScreen).GetField("_targetDragPos",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (distYField == null || targetField == null) return;

        int maxRow = (runState.Map?.GetRowCount() - 1) ?? 6;
        int lastRow = (runState.ActFloor - 1);
        if (lastRow < 0) lastRow = 0;
        if (lastRow > maxRow) lastRow = maxRow;

        float distY = (float)distYField.GetValue(__instance);
        float targetY = -600f + (float)lastRow * distY;
        targetField.SetValue(__instance, new Vector2(0f, targetY));

        GD.Print($"[Act4Fix] Map visited coords empty; ActFloor={runState.ActFloor} cam->row {lastRow} (y={targetY})");
    }

    // ──────────────────────────────────────────
    // 3. RecalculateTravelability → 修正 Boss 状态
    // ──────────────────────────────────────────
    [HarmonyPatch(typeof(NMapScreen), "RecalculateTravelability")]
    [HarmonyPrefix]
    static bool Recaltravel_Prefix(NMapScreen __instance)
    {
        var rsField = typeof(NMapScreen).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (rsField?.GetValue(__instance) is not RunState runState || runState.Act is not Act4)
            return true; // 非 Act4，走原版

        var dictField = typeof(NMapScreen).GetField("_mapPointDictionary",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var bossField = typeof(NMapScreen).GetField("_bossPointNode",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var startField = typeof(NMapScreen).GetField("_startingPointNode",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var mapField = typeof(NMapScreen).GetField("_map",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (dictField?.GetValue(__instance) is not Dictionary<MapCoord, NMapPoint> dict
            || mapField?.GetValue(__instance) is not ActMap map)
            return true;

        var bossNode = bossField?.GetValue(__instance) as NMapPoint;
        var startNode = startField?.GetValue(__instance) as NMapPoint;

        // 全部设为 Untravelable
        foreach (var kv in dict)
            kv.Value.State = MapPointState.Untravelable;

        // 标记 visited 节点
        foreach (var coord in runState.VisitedMapCoords)
        {
            if (dict.TryGetValue(coord, out var n))
                n.State = MapPointState.Traveled;
        }

        if (!runState.VisitedMapCoords.Any())
        {
            if (runState.ActFloor > 0)
            {
                // visited 被清空但已走过至少一间房 → 玩家在火堆/最后房间
                // 把起点标记为已走，Boss 设为可达
                if (startNode != null)
                    startNode.State = MapPointState.Traveled;
                if (bossNode != null)
                    bossNode.State = MapPointState.Travelable;
                GD.Print($"[Act4Fix] Recaltravel: visited empty, ActFloor={runState.ActFloor} => Boss travelable");
            }
            else
            {
                // 首次进入 Act4 → 只开起点
                if (startNode != null)
                    startNode.State = MapPointState.Travelable;
            }
            return false;
        }

        // 正常原版逻辑：从 lastCoord 找子节点
        var lastCoord = runState.VisitedMapCoords.Last();
        if (lastCoord.row == map.GetRowCount() - 1 && bossNode != null)
        {
            bossNode.State = MapPointState.Travelable;
            return false;
        }

        // 子节点设为 Travelable
        if (dict.TryGetValue(lastCoord, out var lastNode))
        {
            foreach (var child in lastNode.Point.Children)
            {
                if (dict.TryGetValue(child.coord, out var cn))
                    cn.State = MapPointState.Travelable;
            }
        }

        return false;
    }
}