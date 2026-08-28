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
    private static bool _firstEliteGemGranted;

    internal static bool Internal_FirstEliteGemGranted
    {
        get => _firstEliteGemGranted;
        set => _firstEliteGemGranted = value;
    }

    public static bool HasAllFourGems(IRunState runState)
    {
        // 检测所有玩家携带的遗物集合是否集齐4种宝石（不要求同一个人持有）
        var allRelics = runState.Players.SelectMany(p => p.Relics).ToList();
        return allRelics.Any(r => r is ScoutHospitality)
            && allRelics.Any(r => r is ScoutPerseverance)
            && allRelics.Any(r => r is ScoutAmbition)
            && allRelics.Any(r => r is ScoutEnterprise);
    }

    public static void Reset() { _firstEliteGemGranted = false; }
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

        // 精英 — 首次精英战注入 ScoutAmbition
        if (room is CombatRoom combatRoom && combatRoom.RoomType == RoomType.Elite
            && !Act4GemManagerPatch.Internal_FirstEliteGemGranted)
        {
            if (!runState.Players.Any(p => p.Relics.Any(r => r is ScoutAmbition)))
            {
                var relic = ModelDb.Relic<ScoutAmbition>().ToMutable();
                __result.Rewards.Add(new RelicReward(relic, player));
                Act4GemManagerPatch.Internal_FirstEliteGemGranted = true;
                GD.Print("[Act4Gem] Elite: ScoutAmbition added as manual relic reward");
            }
        }

        // Boss（Act2）— 仅给当前玩家注入 ScoutEnterprise（类似精英宝石机制）
        if (room is CombatRoom bossRoom && bossRoom.RoomType == RoomType.Boss
            && runState.CurrentActIndex == 1)
        {
            // 只检查当前玩家是否已有，而不是检查所有玩家
            if (!player.Relics.Any(r => r is ScoutEnterprise))
            {
                var relic = ModelDb.Relic<ScoutEnterprise>().ToMutable();
                __result.Rewards.Add(new RelicReward(relic, player));
                GD.Print("[Act4Gem] Boss: ScoutEnterprise added as manual relic reward");
            }
        }
    }
}