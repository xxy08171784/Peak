using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
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

    // ==== 一次性宝石的发放记录（"不拿就没有"）====
    // 精英宝石只在本局第一场精英战发放：同一房间内的所有玩家都算同一次机会，
    // 之后的精英房一律不再发；发放过就算消耗掉，玩家放弃也拿不到了。
    private static IRunState? _trackedRun;
    private static AbstractRoom? _eliteGemOfferedRoom;

    /// <summary>本局是否该发放精英宝石：第一次遇到的那场精英给，之后不再给。</summary>
    internal static bool ShouldOfferEliteGem(IRunState runState, AbstractRoom room)
    {
        SyncRun(runState);
        return _eliteGemOfferedRoom == null || ReferenceEquals(_eliteGemOfferedRoom, room);
    }

    /// <summary>标记本场精英已发放过精英宝石。</summary>
    internal static void MarkEliteGemOffered(IRunState runState, AbstractRoom room)
    {
        SyncRun(runState);
        _eliteGemOfferedRoom = room;
    }

    /// <summary>换局（新的一局 / 读档）时清空一次性发放记录。</summary>
    private static void SyncRun(IRunState runState)
    {
        if (!ReferenceEquals(_trackedRun, runState))
        {
            _trackedRun = runState;
            _eliteGemOfferedRoom = null;
        }
    }
}

/// <summary>
/// 记录 Boss / 精英战里"补刀"的玩家，供宝石发放判定（#3 野心 / #4 进取）。
///
/// 原版 <see cref="CreatureCmd.Kill(Creature, bool)"/> 明确把击杀归属给"整个敌对阵营"
/// （见其注释），不区分谁打出致命一击，所以这里自己在收到致命伤的那一刻记录 dealer。
///
/// ⚠ 为什么挂 <see cref="Hook.AfterDamageGiven"/> 而不是 <see cref="CombatHistory.DamageReceived"/>：
/// 后者在 CreatureCmd.Damage 里被 `!CombatManager.IsEnding` 挡着（CreatureCmd.cs:317），而
/// IsEnding 是计算属性（CombatManager.cs:180）——死亡发生的瞬间（LoseHpInternal 之后）场上
/// 已无存活主敌人，IsEnding 立即为 true，于是**致死那一击根本不会写进战斗历史**，
/// 追踪器永远看不到它（非致死伤害倒都会记录，所以表面上像是好的）。
/// <see cref="Hook.AfterDamageGiven"/>（CreatureCmd.cs:409）没有这道闸门，且在 Kill 之前
/// 调用，`WasTargetKilled` / `dealer` / `target` 参数也齐全。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageGiven))]
public static class KillerTrackerPatch
{
    private static AbstractRoom? _room;
    private static Player? _killer;

    /// <summary>
    /// 该房间 Boss / 精英的击杀者。不是本房间记录的则返回 null。
    /// </summary>
    internal static Player? GetKiller(AbstractRoom room)
        => ReferenceEquals(room, _room) ? _killer : null;

    [HarmonyPostfix]
    static void RecordKiller(ICombatState combatState, Creature? dealer, DamageResult results, Creature target)
    {
        // 只在"击杀主敌人"时记录：副敌人（小怪/召唤物）会被 Kill(teammates) 连带带走，不算补刀
        if (!results.WasTargetKilled || !target.IsPrimaryEnemy)
        {
            return;
        }

        IRunState? runState = combatState?.RunState;
        if (runState?.CurrentRoom is not CombatRoom room
            || (room.RoomType != RoomType.Boss && room.RoomType != RoomType.Elite))
        {
            return;
        }

        // 归属：亲手击杀的玩家；宠物（Osty）的击杀算主人；
        // 毒杀 / 环境伤害 / 自伤等拿不到玩家 dealer 的情况，随机抽一名玩家兜底。
        Player? killer = dealer != null && dealer.IsPlayer ? dealer.Player : dealer?.PetOwner;

        if (killer == null)
        {
            // 用共享的 Run RNG：各客户端跑同一套确定性模拟、此刻 RNG 状态一致，
            // 抽出的玩家必然相同，不会出现"各端认为中奖者不同"的不同步。
            List<Player> pool = runState.Players.Where(p => p.Creature.IsAlive).ToList();
            if (pool.Count == 0)
            {
                pool = runState.Players.ToList();
            }

            killer = pool[runState.Rng.CombatTargets.NextInt(0, pool.Count)];
        }

        _room = room;
        _killer = killer;
    }
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

        // 精英宝石（野心）：只在本局第一场精英战发放一次，不拿就没有（不再补发）。
        // 只发给"补刀"那名玩家：多人下每人各自生成 RewardsSet，用记录的击杀者过滤。
        // 未命中的玩家不会 MarkEliteGemOffered，所以轮到补刀者那次仍能正常拿到。
        if (room is CombatRoom combatRoom && combatRoom.RoomType == RoomType.Elite
            && !Act4GemManagerPatch.TeamHas<ScoutAmbition>(runState)
            && Act4GemManagerPatch.ShouldOfferEliteGem(runState, room)
            && KillerTrackerPatch.GetKiller(room) == player)
        {
            var relic = ModelDb.Relic<ScoutAmbition>().ToMutable();
            __result.Rewards.Add(new RelicReward(relic, player));
            Act4GemManagerPatch.MarkEliteGemOffered(runState, room);
        }

        // Boss 宝石（进取）：只在原版第二幕（Act2，index 1）Boss 战发放，且只发一次 —— 不拿就没有。
        // 只发给"补刀"那名玩家：多人下每个人都会生成自己的 RewardsSet，
        // 这里用 KillerTrackerPatch 记录的击杀者做过滤。
        if (room is CombatRoom bossRoom && bossRoom.RoomType == RoomType.Boss
            && runState.CurrentActIndex == 1
            && !Act4GemManagerPatch.TeamHas<ScoutEnterprise>(runState)
            && KillerTrackerPatch.GetKiller(room) == player)
        {
            var relic = ModelDb.Relic<ScoutEnterprise>().ToMutable();
            __result.Rewards.Add(new RelicReward(relic, player));
        }
    }
}