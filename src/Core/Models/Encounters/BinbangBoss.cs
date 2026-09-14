using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Encounters;

/// <summary>
/// 隐藏 Boss 遭遇战 —— 宾邦（三命形态：常态 / 黄金 / 黑化）。
/// 作为第四层地图的"第二个 Boss"：好结局（持有童军的荣耀）后地图上才会出现该节点，
/// 由原版双 Boss 流程引导玩家走到这里（见 NadirActMap.SecondBossMapPoint）。
/// </summary>
public sealed class BinbangBoss : EncounterModel
{
    public override RoomType RoomType => RoomType.Boss;

    /// <summary>
    /// 复用原版占位 Boss 图标（与 Act4Boss 相同），避免 NBossMapPoint 加载不存在的资源。
    /// </summary>
    public override string BossNodePath => "res://images/map/placeholder/test_subject_boss_icon";

    public override IEnumerable<MonsterModel> AllPossibleMonsters
        => new[] { ModelDb.Monster<Monsters.Binbang>() };

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        => new[] { (ModelDb.Monster<Monsters.Binbang>().ToMutable(), null as string) };
}
