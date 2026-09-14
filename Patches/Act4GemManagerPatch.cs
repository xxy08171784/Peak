using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Rewards;
using peak.Core.Models.Relics;
using peak.Core.Models.RestSites;

namespace peak.Patches;

/// <summary>
/// Manages the 4 gem relics distribution.
/// </summary>
public static class Act4GemManagerPatch
{
    /// <summary>
    /// 队伍（所有玩家遗物并集）是否已持有某种宝石。
    /// 宝石是"队伍共享收集品"，所有发放/判定都按并集来，避免多人下判定不一致。
    /// </summary>
    internal static bool TeamHas<T>(IRunState runState) where T : RelicModel
        => runState.Players.Any(p => p.Relics.Any(r => r is T));

    public static bool HasAllFourGems(IRunState runState)
        => TeamHas<ScoutHospitality>(runState)
        && TeamHas<ScoutPerseverance>(runState)
        && TeamHas<ScoutAmbition>(runState)
        && TeamHas<ScoutEnterprise>(runState);
}

/// <summary>
/// 商店宝石 (#1 ScoutHospitality)：
/// 替换 PopulateRelicEntries 生成的第三个遗物（原为普通 Shop 遗物）为宝石遗物。
/// 不增加总槽数，无需动态加槽。
/// </summary>
[HarmonyPatch(typeof(MerchantInventory), "PopulateRelicEntries")]
public static class MerchantRelicReplacePatch
{
    [HarmonyPostfix]
    static void ReplaceThirdRelicWithGem(MerchantInventory __instance)
    {
        if (__instance == null) return;
        if (__instance.Player.RunState.Players.Any(p =>
            p.Relics.Any(r => r is ScoutHospitality))) return;

        var relic = ModelDb.Relic<ScoutHospitality>().ToMutable();
        var entries = Traverse.Create(__instance).Field("_relicEntries").GetValue<List<MerchantRelicEntry>>();
        if (entries != null && entries.Count >= 3)
        {
            entries[2] = new MerchantRelicEntry(relic, __instance.Player);
            GD.Print("[Act4Gem] Shop: ScoutHospitality replaced slot #3");
        }
    }
}

/// <summary>
/// 火堆宝石 (#2 ScoutPerseverance)：
/// 在火堆「坚定」选项中获取（已有补丁不变）。
/// </summary>
[HarmonyPatch(typeof(RestSiteOption), "Generate")]
public static class RestSiteFirmOptionPatch
{
    [HarmonyPostfix]
    static void AddFirmOption(Player player,
        ref System.Collections.Generic.List<RestSiteOption> __result)
    {
        if (player == null || __result == null) return;
        if (player.RunState.Players.Any(p =>
            p.Relics.Any(r => r is ScoutPerseverance))) return;

        var opt = new FirmRestSiteOption(player);
        if (opt.IsEnabled) __result.Add(opt);
    }
}

/// <summary>
/// 精英宝石 (#3 ScoutAmbition) + Boss宝石 (#4 ScoutEnterprise)：
/// 在结算奖励界面注入 RelicReward，让玩家手动拾取。
/// </summary>
[HarmonyPatch(typeof(RewardsSet), "WithRewardsFromRoom")]
public static class EliteBossGemRewardPatch
{
    [HarmonyPostfix]
    static void InjectGemRelicRewards(RewardsSet __result, AbstractRoom room)
    {
        if (__result?.Player == null) return;
        var player = __result.Player;
        var runState = player.RunState;

        // 精英宝石（野心）：任何精英战都会补发，直到队伍持有为止。
        // 旧实现是"仅首次精英 + 一次性静态标记"，玩家只要放弃那次奖励，
        // 宝石就永久拿不到（进不了第四层）；改为按持有情况补发。
        if (room is CombatRoom combatRoom && combatRoom.RoomType == RoomType.Elite
            && !Act4GemManagerPatch.TeamHas<ScoutAmbition>(runState))
        {
            var relic = ModelDb.Relic<ScoutAmbition>().ToMutable();
            __result.Rewards.Add(new RelicReward(relic, player));
        }

        // Boss 宝石（进取）：第二幕起的每个 Boss 战都会补发，直到队伍持有为止
        // （同样修掉"放弃即永久锁死"的问题；第四层 Boss 无需再发，因为那时宝石必然已齐）。
        if (room is CombatRoom bossRoom && bossRoom.RoomType == RoomType.Boss
            && runState.CurrentActIndex >= 1
            && runState.CurrentActIndex <= 2
            && !Act4GemManagerPatch.TeamHas<ScoutEnterprise>(runState))
        {
            var relic = ModelDb.Relic<ScoutEnterprise>().ToMutable();
            __result.Rewards.Add(new RelicReward(relic, player));
        }
    }
}