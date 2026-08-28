using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

namespace peak.Core.Models.Acts;

/// <summary>
/// 第4幕 - 自定义隐藏层。
/// 通过 Act4TransitionPatch 在第3幕Boss战后检测4颗宝石注入。
/// </summary>
public sealed class Act4 : ActModel
{
    public override int Index => 3;
    public override bool IsDefault => false;
    // StandardActMap 最小可行行数：_mapLength = BaseNumberOfRooms + 1，要求 _mapLength-7 >= 0
    protected override int BaseNumberOfRooms => 6;
    protected override int NumberOfWeakEncounters => 2;

    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    public override string[] BgMusicOptions => new[] { "event:/music/act2_a1_v2" };
    public override string[] MusicBankPaths => new[] { "res://banks/desktop/act2_a1.bank" };
    public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";
    public override string ChestSpineSkinNameNormal => "act3";
    public override string ChestSpineSkinNameStroke => "act3_stroke";

    /// <summary>
    /// 覆盖 ChestSpineResourcePath → 使用 act3 的宝箱骨胳资源。
    /// 默认指向 chest_room_act_act4_skel_data.tres（不存在）。
    /// </summary>
    public override string ChestSpineResourcePath =>
        "res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres";

    public override Color MapTraveledColor => new Color("2A1B3D");
    public override Color MapUntraveledColor => new Color("6B3FA0");
    public override Color MapBgColor => new Color("9B59B6");

    public override IEnumerable<EncounterModel> BossDiscoveryOrder =>
        new EncounterModel[] { ModelDb.Encounter<Encounters.Act4Boss>() };

    public override IEnumerable<AncientEventModel> AllAncients =>
        System.Array.Empty<AncientEventModel>();

    public override IEnumerable<EventModel> AllEvents =>
        System.Array.Empty<EventModel>();

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
        new EncounterModel[]
        {
            ModelDb.Encounter<Encounters.Act4Boss>(),
            // StandardActMap 会生成大量 Monster/Elite 行，必须有填充遭遇战（官方怪复用）
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.SlimesNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.ChompersNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.BygoneEffigyElite>(),
        };

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) =>
        System.Array.Empty<AncientEventModel>();

    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState) { }

    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
    {
        // StandardActMap 硬编码：Row0=Ancient(Postfix 改 Shop), Row1=Monster,
        // Row(mapLength-7)=Treasure(此配置下为空行), Row(mapLength-1)=RestSite
        // 中间行按此处计数分配，剩余未分配行 → Monster
        return new MapPointTypeCounts(unknownCount: 1, restCount: 1);
    }
}