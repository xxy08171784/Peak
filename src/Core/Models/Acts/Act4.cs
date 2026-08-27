using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core;
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
    protected override int BaseNumberOfRooms => 8;
    protected override int NumberOfWeakEncounters => 0;

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

    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        // 填充遭遇战（安全网）：Act4MapPatch Postfix 会把所有 Monster→Shop，
        // 这些 encounter 永远不会实际进入，但 RoomSet 需要非空列表避免除零崩溃。
        return new EncounterModel[]
        {
            ModelDb.Encounter<Encounters.Act4Boss>(),
#pragma warning disable CS0618 // ModelDb.Encounter<T> 可能标记了 Obsolete
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.SlimesNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.ChompersNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.MytesNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.FlyconidNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.CultistsNormal>(),
            ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.SlimesWeak>(),
#pragma warning restore CS0618
        };
    }

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) =>
        System.Array.Empty<AncientEventModel>();

    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState) { }

    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
    {
        // 地图结构：Start(Shop) -> Shop -> Treasure -> 随机 -> RestSite -> Boss
        // BaseNumberOfRooms=8 → _mapLength=9 (行0~8)
        // 硬编码：Row1=Monster(→Shop), Row2=Treasure, Row8=RestSite
        // 其余行由默认 MapPointTypeCounts 分配
        // Act4MapPatch Postfix 会把 Monster→Shop, Elite→Treasure
        return new MapPointTypeCounts(unknownCount: 1, restCount: 2);
    }
}