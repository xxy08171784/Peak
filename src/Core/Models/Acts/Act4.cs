using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using peak.Core.Map;

namespace peak.Core.Models.Acts;

/// <summary>
/// 第4幕「天底」—— 隐藏层（宝石走廊）。
///
/// 继承 BaseLib 的 <see cref="CustomActModel"/>：
///   - 构造函数 base(4, autoAdd:false)：actNumber=4 → Index=3；autoAdd=false 表示**不**让 BaseLib
///     把 Act4 注入 ModelDb.Acts（否则每一局都会多出第4幕，且 GetDefaultList 会因 index 3
///     没有 IsDefault 的 act 而抛异常）。改为由 Patches/Act4EntryPatch 在集齐4颗宝石后按需注入。
///   - 地图通过 CustomCreateMap 换成直线走廊 <see cref="NadirActMap"/>：一层商店 → 一层篝火 → Boss（无战斗节点）。
///   - 地图/背景/篝火场景资源通过 CustomAct*Path 覆盖指向 act3(glory)，暂不为四层单独做美术。
/// </summary>
public sealed class Act4 : CustomActModel
{
    public Act4()
        : base(4, autoAdd: false)
    {
    }

    // ===== 必须覆盖：非原版 index 3，基类默认 AllAncients 会抛异常 =====
    // 本层没有 Ancient 事件（起点直接就是商店）。GenerateRooms 对空 ancient 池取 NextItem
    // 会得到 null，不会崩溃；地图上也没有 Ancient 节点，PullAncient 不会被调用。
    public override IEnumerable<AncientEventModel> AllAncients => System.Array.Empty<AncientEventModel>();

    /// <summary>走廊只有"商店 + 篝火"两个房间（不含 Boss）。自定义地图不使用这个值，仅保持语义正确。</summary>
    protected override int BaseNumberOfRooms => 2;

    // 走廊地图不含 Event 节点（原版只有 Ancient 节点会进 EventRoom），暂不需要事件。
    public override IEnumerable<EventModel> AllEvents => System.Array.Empty<EventModel>();

    public override IEnumerable<EncounterModel> BossDiscoveryOrder =>
        new[] { ModelDb.Encounter<Encounters.Act4Boss>() };

    // ===== 资源覆盖：全部指向 act3(glory)，避免为四层单独做美术 =====
    protected override string CustomMapTopBgPath =>
        ImageHelper.GetImagePath("packed/map/map_bgs/glory/map_top_glory.png");

    protected override string CustomMapMidBgPath =>
        ImageHelper.GetImagePath("packed/map/map_bgs/glory/map_middle_glory.png");

    protected override string CustomMapBotBgPath =>
        ImageHelper.GetImagePath("packed/map/map_bgs/glory/map_bottom_glory.png");

    protected override string CustomRestSiteBackgroundPath =>
        SceneHelper.GetScenePath("rest_site/glory_rest_site");

    // 地图配色（沿用旧的紫色，便于和 act1~3 区分）
    public override Color MapTraveledColor => new Color("2A1B3D");
    public override Color MapUntraveledColor => new Color("6B3FA0");
    public override Color MapBgColor => new Color("9B59B6");

    /// <summary>
    /// 用走廊地图替换标准地图：无战斗/精英/宝箱/未知节点，只有 商店/篝火。
    /// 这是 BaseLib 提供的正规地图入口（prefix 掉 ActModel.CreateMap）。
    /// </summary>
    protected override ActMap? CustomCreateMap(RunState runState, bool replaceTreasureWithElites)
        => new NadirActMap(runState);

    /// <summary>
    /// 走廊没有普通/精英战斗节点，列表为空也不会被消费（AddWithoutRepeatingTags 对空袋安全）。
    /// 只保留 Boss，避免像旧实现那样塞一堆无关的原版遭遇当"安全网"。
    /// </summary>
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
#pragma warning disable CS0618 // ModelDb.Encounter<T> 可能标记了 Obsolete
        return new[] { ModelDb.Encounter<Encounters.Act4Boss>() };
#pragma warning restore CS0618
    }

    /// <summary>
    /// 自定义地图不经过 StandardActMap，这里仅保证该抽象方法有实现（返回值不会被使用）。
    /// </summary>
    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
        => new MapPointTypeCounts(unknownCount: 0, restCount: 0);

    public override bool IsUnlocked(UnlockState unlockState) => true;
}
