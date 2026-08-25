using System.Collections.Generic;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Encounters;

/// <summary>
/// 第4幕 Boss 遭遇战 - 领队迈尔斯。
/// 占位实现，待Boss完整逻辑实现后再补充自定义场景/BGM等。
/// </summary>
public sealed class Act4Boss : EncounterModel
{
    public override RoomType RoomType => RoomType.Boss;

    /// <summary>
    /// 覆盖 BossNodePath → 使用 Glory act3 Boss 占位图标资源。
    /// 默认路径指向 res://animations/map/act4_boss/... 不存在导致 NBossMapPoint 崩溃。
    /// NBossMapPoint._Ready() 对 placeholders 加载 {BossNodePath}.png 和 _outline.png。
    /// </summary>
    public override string BossNodePath => "res://images/map/placeholder/test_subject_boss_icon";

    public override IEnumerable<MonsterModel> AllPossibleMonsters
        => new[] { ModelDb.Monster<Monsters.LeaderMiles>() };

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        => new[] { (ModelDb.Monster<Monsters.LeaderMiles>().ToMutable(), null as string) };
}