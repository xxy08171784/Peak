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
/// 4 颗宝石遗物的发放管理。
///
/// 发放渠道（关键）：Boss / 精英宝石登记进 <see cref="CombatRoom.ExtraRewards"/>，而不是只往
/// 当前奖励界面里临时塞一条。ExtraRewards 是原版给 TheHunt / HeistPower 这类"这场打完再给你个奖励"
/// 用的通道：它会按玩家 NetId 写进 SerializableRoom.ExtraRewards 随存档持久化
/// （RelicReward 靠 PredeterminedModelId 能完整还原），读档后重进同一个房间时
/// <see cref="RewardsSet.WithRewardsFromRoom"/> 会把它们重新加回奖励界面。
/// 于是"打完 Boss 没点宝石就退出/掉线/闪退"不再等于永久失去这颗宝石 —— 之前那版只往
/// Rewards 里塞一条，读档重进时奖励界面虽然会重新生成（金/卡奖励都回来了），宝石却回不来。
/// </summary>
public static class Act4GemManagerPatch
{
    /// <summary>某个玩家自己是否持有某种宝石。</summary>
    internal static bool PlayerHas<T>(Player player) where T : RelicModel
        => player.Relics.Any(r => r is T);

    /// <summary>
    /// 队伍（所有玩家遗物并集）是否已持有某种宝石。
    /// 宝石是"队伍共享收集品"，所有发放/判定都按并集来，避免多人下判定不一致。
    /// </summary>
    internal static bool TeamHas<T>(IRunState runState) where T : RelicModel
        => runState.Players.Any(PlayerHas<T>);

    public static bool HasAllFourGems(IRunState runState)
        => TeamHas<ScoutHospitality>(runState)
        && TeamHas<ScoutPerseverance>(runState)
        && TeamHas<ScoutAmbition>(runState)
        && TeamHas<ScoutEnterprise>(runState);

    // ==== 一次性宝石的发放记录（"不拿就没有"）====
    // 精英宝石只在本局第一场精英战发放：同一房间内的所有玩家都算同一次机会，
    // 之后的精英房一律不再发。
    // 注：这个标记只在内存里，读档会丢 —— 那正是想要的：读档重进同一个精英房时还要能把
    // 宝石补回来（见 GrantGemReward）。代价是"跳过后读档、再去打下一个精英"会被再发一次机会；
    // 因为已经拿到就会剔除（PruneOwnedGemReward），最多是把少拿一次变成多给一次机会。
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

    /// <summary>
    /// 把宝石登记成本房间的额外奖励（随存档持久化），并加进这一次的奖励界面。
    /// 同一个房间重复生成奖励界面（读档再进）时不会重复登记。
    /// </summary>
    internal static void GrantGemReward<T>(CombatRoom room, Player player, RewardsSet result) where T : RelicModel
    {
        if (RoomHasGemReward<T>(room, player))
        {
            return;
        }

        RelicModel relic = ModelDb.Relic<T>().ToMutable();
        RelicReward reward = new RelicReward(relic, player);
        room.AddExtraReward(player, reward);
        result.Rewards.Add(reward);
        GD.Print($"[Act4Gem] 登记宝石奖励 {relic.Id} -> 玩家 {player.NetId}（{room.RoomType} 房）");
    }

    /// <summary>本房间是否已经给该玩家登记过这颗宝石。</summary>
    private static bool RoomHasGemReward<T>(CombatRoom room, Player player) where T : RelicModel
        => room.ExtraRewards.TryGetValue(player, out List<Reward> rewards)
        && rewards.Any(r => r is RelicReward relicReward && relicReward.Relic is T);

    /// <summary>
    /// 确定宝石的接收者：不再做"击杀判定"。
    /// 单人 = 唯一玩家；多人 = 用队伍级确定性 RNG 抽一名存活玩家（无人存活则从全体抽）。
    ///
    /// 关键：多人下每个客户端都会调用本方法，由于联机是确定性模拟、此刻 RNG 状态一致，
    /// 抽出的接收者必然相同 —— 因此只有抽中的那一方会真正登记宝石，天然只发一份，
    /// 不会出现"各端以为中奖者不同"或"发给多个人"的不同步。
    ///
    /// ⚠ 为什么这能顺带修复"读档后宝石消失"：旧的击杀判定依赖内存里的 _killer，
    /// 读档后内存清空使 GetKiller 返回 null，导致没有任何玩家能把已持久化在房间
    /// ExtraRewards 里的宝石重新加回奖励界面（看注释：GetKiller 只在内存、读档会丢）。
    /// 这里用运行时 RNG + 存活玩家列表，读档重进同一房间时只要"Boss/精英房 + 队伍未持有"，
    /// 就总能抽出一个接收者，宝石能正常回归奖励界面。
    /// </summary>
    internal static Player PickGemRecipient(IRunState runState)
    {
        List<Player> pool = runState.Players.Where(p => p.Creature.IsAlive).ToList();
        if (pool.Count == 0)
        {
            pool = runState.Players.ToList();
        }
        return pool[runState.Rng.CombatTargets.NextInt(0, pool.Count)];
    }

    /// <summary>
    /// 玩家已经持有这颗宝石时，把奖励界面里的它剔除。
    /// 房间额外奖励是持久的（读档重进还会再带出来一次），而 <c>Player.AddRelicInternal</c>
    /// 没有去重保护，不剔除就能拿到第二颗 —— 那会变成战斗开始时多吃一次敏捷。
    /// </summary>
    internal static void PruneOwnedGemReward<T>(RewardsSet result, Player player) where T : RelicModel
    {
        if (!PlayerHas<T>(player))
        {
            return;
        }
        if (result.Rewards.RemoveAll(r => r is RelicReward relicReward && relicReward.Relic is T) > 0)
        {
            GD.Print($"[Act4Gem] 玩家 {player.NetId} 已持有 {typeof(T).Name}，剔除重复的宝石奖励");
        }
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
/// 接收者随机抽取（单人=该玩家，多人=各端确定性同步抽中的同一名玩家），
/// 奖励登记进房间的 ExtraRewards（随存档持久化，读档重进还能补拿）。
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

        // 已经拿过的宝石：奖励界面里剔掉（房间额外奖励读档重进会再带出来一次，不做去重会拿到第二颗）
        Act4GemManagerPatch.PruneOwnedGemReward<ScoutAmbition>(__result, player);
        Act4GemManagerPatch.PruneOwnedGemReward<ScoutEnterprise>(__result, player);

        if (room is not CombatRoom combatRoom) return;

        // 精英宝石（野心）：只在本局第一场精英战发放一次。
        // 接收者用随机抽取（单人=该玩家，多人=所有客户端确定性同步的同一名玩家）。
        // 未命中的玩家不会 MarkEliteGemOffered，所以轮到接收者那次仍能正常拿到。
        if (combatRoom.RoomType == RoomType.Elite
            && !Act4GemManagerPatch.TeamHas<ScoutAmbition>(runState)
            && Act4GemManagerPatch.ShouldOfferEliteGem(runState, combatRoom)
            && Act4GemManagerPatch.PickGemRecipient(runState) == player)
        {
            Act4GemManagerPatch.GrantGemReward<ScoutAmbition>(combatRoom, player, __result);
            Act4GemManagerPatch.MarkEliteGemOffered(runState, combatRoom);
        }

        // Boss 宝石（进取）：只在原版第二幕（Act2，index 1）Boss 战发放，且只发一次。
        // 接收者用随机抽取（单人=该玩家，多人=确定性同步的同一名玩家），摆脱击杀判定。
        if (combatRoom.RoomType == RoomType.Boss
            && runState.CurrentActIndex == 1
            && !Act4GemManagerPatch.TeamHas<ScoutEnterprise>(runState)
            && Act4GemManagerPatch.PickGemRecipient(runState) == player)
        {
            Act4GemManagerPatch.GrantGemReward<ScoutEnterprise>(combatRoom, player, __result);
        }
    }
}
