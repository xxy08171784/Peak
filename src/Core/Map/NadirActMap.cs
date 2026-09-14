using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.Relics;

namespace peak.Core.Map;

/// <summary>
/// 第4幕「天底」的走廊地图：**商店（起点）→ 篝火 → Boss**，一条直线。
///
/// 与标准地图的区别：
///   - 没有 战斗/精英/宝箱/未知 节点（Unknown 可能掷出战斗房间，与"纯抉择走廊"冲突）；
///   - 没有分支，只有两个节点加一个 Boss。
///
/// 结构（列,行），7 列 × 2 行，Boss 在第 2 行（原版惯例：Boss 坐标在网格之外）：
/// <code>
///        Boss(2,2)
///           |
///        篝火(2,1)
///           |
///        商店(2,0)  ← 起点（原版开局唯一可点的节点）
/// </code>
///
/// 全部放在第 2 列：原版 <c>NMapScreen.SetMap</c> 把 Boss 固定在 x=-200，而节点 x = -500 + 150*col，
/// col=2 恰好是 x=-200 —— 整条走廊与 Boss 自然对齐，不需要任何挪位补丁。
/// （起点节点由原版固定在 x=-80，会略微偏右，属正常表现。）
/// </summary>
public sealed class NadirActMap : ActMap
{
    private const int ColumnCount = 7;
    private const int GridRowCount = 2;   // 行 0 商店（起点）、行 1 篝火；Boss 在第 2 行

    private readonly IRunState _runState;
    private MapPoint? _secondBossPoint;

    public override MapPoint BossMapPoint { get; }

    public override MapPoint StartingMapPoint { get; }

    /// <summary>
    /// 隐藏 Boss（第二个 Boss）节点。
    /// 只在队伍拿到「童军的荣耀」（第四层好结局）后才有值——好结局那一刻
    /// ScoutGlory.AfterObtained 会重渲地图（NMapScreen.SetMap，原版 MapCmd 同样这么做），
    /// 节点随之出现。之后原版双 Boss 流程接管：Boss1 结算后走地图 → 隐藏 Boss → 走建筑师结算。
    /// 普通结局时为 null，Boss1 结算后直接换幕，不会有第二个 Boss。
    /// </summary>
    public override MapPoint? SecondBossMapPoint
        => HasScoutGlory ? (_secondBossPoint ??= CreateSecondBossPoint()) : null;

    protected override MapPoint?[,] Grid { get; }

    public NadirActMap(IRunState runState)
    {
        _runState = runState;

        Grid = new MapPoint?[ColumnCount, GridRowCount];

        // 起点直接就是商店（原版开局唯一可点的是起点节点 → 第一个房间就是商店）。
        // 注意：必须直接 new、**不能**经 Put 放进 Grid —— 原版 StandardActMap 的起点节点
        // 就是独立于 Grid 的（Grid 只存普通节点）。一旦放进 Grid，GetAllMapPoints() 会连它
        // 一起返回，SetMap 便会对同一条边 (2,0)→(2,1) 调用两次 DrawPaths，
        // _paths.Add 随即抛 "An item with the same key has already been added"。
        StartingMapPoint = new MapPoint(2, 0) { PointType = MapPointType.Shop };
        MapPoint rest = Put(2, 1, MapPointType.RestSite);
        BossMapPoint = new MapPoint(2, GridRowCount) { PointType = MapPointType.Boss };

        StartingMapPoint.AddChildPoint(rest);
        rest.AddChildPoint(BossMapPoint);

        startMapPoints.Add(StartingMapPoint);
    }

    /// <summary>原版 StandardActMap 把第二个 Boss 放在第一个 Boss 的上一行，并与之相连。</summary>
    private MapPoint CreateSecondBossPoint()
    {
        var point = new MapPoint(ColumnCount / 2, GridRowCount + 1) { PointType = MapPointType.Boss };
        BossMapPoint.AddChildPoint(point);
        return point;
    }

    private bool HasScoutGlory
        => _runState.Players.SelectMany(p => p.Relics).Any(r => r is ScoutGlory);

    private MapPoint Put(int col, int row, MapPointType type)
    {
        var point = new MapPoint(col, row) { PointType = type };
        Grid[col, row] = point;
        return point;
    }
}
