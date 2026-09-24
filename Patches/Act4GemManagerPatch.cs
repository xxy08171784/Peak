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
/// 记录 Boss / 精英战里"补刀"的玩家，供宝石发放判定（#3 野心 / #4 进取）。
///
/// 原版 <see cref="CreatureCmd.Kill(Creature, bool)"/> 明确把击杀归属给"整个敌对阵营"
/// （见其注释），不区分谁打出致命一击，所以这里自己在收到致命伤的那一刻记录 dealer。
///
/// ⚠ 不能只看致命伤害：<see cref="Hook.AfterDamageGiven"/> 全游戏只有 <c>CreatureCmd.Damage</c>
/// 会调用（sts2.dll: CreatureCmd.cs:412），而 mod 自己的两种"直接赢"——
/// 【PEAK】(Peak.cs: CreatureCmd.Kill(aliveEnemies)) 和希望达标后的奇迹
/// (HopePower.cs: CreatureCmd.Kill(Owner, force: true))——走的是 CreatureCmd.Kill，
/// 根本不产生伤害事件。只靠致命一击归属的话，用这两者收掉 Boss 就永远记不到击杀者、
/// 宝石奖励也就永远不会被登记（这就是"ScoutEnterprise 经常拿不到"的主因）。
///
/// 所以现在的归属规则是两条：
///   1) 每一次"玩家对主敌人造成伤害"都记住这名玩家（不要求致命）——用 <c>AfterDamageGiven</c>；
///   2) 主敌人死亡时定下本房间的击杀者 = 这一击的 dealer（玩家 / 宠物主人）
///      → 否则用第 1 步记下的最后一个玩家 → 否则随机存活玩家——
///      用 <c>AfterDamageGiven</c>（致命）与 <c>AfterDeath</c>（兜底，覆盖直接击杀）两处都定，
///      后者才让【PEAK】/奇迹这类击杀也能正确归属。
///
/// ⚠ 为什么第 1 步不用 <see cref="CombatHistory.DamageReceived"/>：它在 CreatureCmd.Damage 里
/// 被 `!CombatManager.IsEnding` 挡着（CreatureCmd.cs:317），而 IsEnding 是计算属性
/// （CombatManager.cs:180）——死亡发生的瞬间（LoseHpInternal 之后）场上已无存活主敌人，
/// IsEnding 立即为 true，于是致死那一击根本不会写进战斗历史。AfterDamageGiven 没有这道闸门。
///
/// 记录只存在内存里，读档会丢；这不是问题 —— 宝石一旦被登记进房间的 ExtraRewards 就已经持久化了，
/// 归属只需要在"打完那一场、奖励界面第一次生成"的时候正确一次。
/// </summary>
[HarmonyPatch]
public static class KillerTrackerPatch
{
    private static IRunState? _trackedRun;
    private static CombatRoom? _room;
    private static Player? _lastPlayerDealer;   // 本房间内最后一个对主敌人造成伤害的玩家
    private static Player? _killer;             // 本房间主敌人的击杀者（死亡那一刻定下）
    private static Player? _randomFallback;     // 完全查不到玩家来源时的随机兜底（缓存，保证只抽一次）

    /// <summary>
    /// 该房间 Boss / 精英的击杀者。不是本房间记录的则返回 null。
    /// </summary>
    internal static Player? GetKiller(AbstractRoom room)
        => ReferenceEquals(room, _room) ? (_killer ?? _lastPlayerDealer) : null;

    /// <summary>换局 / 换房间就清空对应记录。</summary>
    private static void TrackRoom(IRunState runState, CombatRoom room)
    {
        if (!ReferenceEquals(_trackedRun, runState))
        {
            _trackedRun = runState;
            _randomFallback = null;
        }
        if (!ReferenceEquals(_room, room))
        {
            _room = room;
            _lastPlayerDealer = null;
            _killer = null;
            _randomFallback = null;
        }
    }

    /// <summary>把 dealer 解析成玩家：亲手打的算自己，宠物（Osty）打的算主人，其它（毒 / 环境 / 自伤）返回 null。</summary>
    private static Player? AsPlayer(Creature? dealer)
        => dealer != null && dealer.IsPlayer ? dealer.Player : dealer?.PetOwner;

    /// <summary>当前是否处于"会发宝石"的房间（Boss / 精英战）。</summary>
    private static bool TryGetGemRoom(IRunState? runState, out CombatRoom? room)
    {
        room = null;
        if (runState?.CurrentRoom is not CombatRoom combatRoom)
        {
            return false;
        }
        if (combatRoom.RoomType != RoomType.Boss && combatRoom.RoomType != RoomType.Elite)
        {
            return false;
        }
        room = combatRoom;
        return true;
    }

    /// <summary>
    /// 定下本房间的击杀者（后死的主敌人说了算）。
    /// </summary>
    private static void ResolveKiller(IRunState runState, Player? lethalDealer)
    {
        Player? killer = lethalDealer ?? _lastPlayerDealer;
        if (killer == null)
        {
            // 用共享的 Run RNG：各客户端跑同一套确定性模拟、此刻 RNG 状态一致，
            // 抽出的玩家必然相同，不会出现"各端认为中奖者不同"的不同步。
            // 结果缓存，避免同一房间重复抽到不同人。
            if (_randomFallback == null)
            {
                List<Player> pool = runState.Players.Where(p => p.Creature.IsAlive).ToList();
                if (pool.Count == 0)
                {
                    pool = runState.Players.ToList();
                }
                _randomFallback = pool[runState.Rng.CombatTargets.NextInt(0, pool.Count)];
            }
            killer = _randomFallback;
        }

        if (_killer != killer)
        {
            _killer = killer;
            GD.Print($"[Act4Gem] 本房间（{_room?.RoomType}）击杀归属 -> 玩家 {killer.NetId}");
        }
    }

    /// <summary>
    /// 记录"玩家对主敌人造成了伤害"——不要求致命，这样【PEAK】/奇迹这类直接击杀
    /// 至少还能用"最后一个打过主敌人的玩家"来归属。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageGiven))]
    static void RecordDamageDealer(ICombatState combatState, Creature? dealer, DamageResult results, Creature target)
    {
        // 只看"主敌人"：副敌人（小怪/召唤物）会被 Kill(teammates) 连带带走，不算补刀
        if (!target.IsPrimaryEnemy)
        {
            return;
        }

        IRunState? runState = combatState?.RunState;
        if (!TryGetGemRoom(runState, out CombatRoom? room) || room == null || runState == null)
        {
            return;
        }
        TrackRoom(runState, room);

        Player? dealerPlayer = AsPlayer(dealer);
        if (dealerPlayer != null)
        {
            _lastPlayerDealer = dealerPlayer;
        }

        // 致命的那一击：直接定归属
        if (results.WasTargetKilled)
        {
            ResolveKiller(runState, dealerPlayer);
        }
    }

    /// <summary>
    /// 主敌人死亡时兜底定归属。<see cref="CreatureCmd.Kill(Creature, bool)"/> 这类直接击杀
    /// 不会触发 AfterDamageGiven（见类注释），只能在这里把击杀者补上。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
    static void RecordDeath(IRunState runState, Creature creature, bool wasRemovalPrevented)
    {
        // removal 被阻止的（复活类效果）不算死；玩家自己死了也不需要记
        if (wasRemovalPrevented || !creature.IsPrimaryEnemy)
        {
            return;
        }
        if (!TryGetGemRoom(runState, out CombatRoom? room) || room == null)
        {
            return;
        }
        TrackRoom(runState, room);

        // 后死的主敌人说了算：每次主敌人死亡都重定一次（伤害击杀在这一刻与致命一击的 dealer 一致）
        ResolveKiller(runState, null);
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
/// 只发给"补刀"那名玩家，奖励登记进房间的 ExtraRewards（随存档持久化，读档重进还能补拿）。
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
        // 只发给"补刀"那名玩家：多人下每人各自生成 RewardsSet，用记录的击杀者过滤。
        // 未命中的玩家不会 MarkEliteGemOffered，所以轮到补刀者那次仍能正常拿到。
        if (combatRoom.RoomType == RoomType.Elite
            && !Act4GemManagerPatch.TeamHas<ScoutAmbition>(runState)
            && Act4GemManagerPatch.ShouldOfferEliteGem(runState, combatRoom)
            && KillerTrackerPatch.GetKiller(combatRoom) == player)
        {
            Act4GemManagerPatch.GrantGemReward<ScoutAmbition>(combatRoom, player, __result);
            Act4GemManagerPatch.MarkEliteGemOffered(runState, combatRoom);
        }

        // Boss 宝石（进取）：只在原版第二幕（Act2，index 1）Boss 战发放，且只发一次。
        // 只发给"补刀"那名玩家：多人下每个人都会生成自己的 RewardsSet，
        // 这里用 KillerTrackerPatch 记录的击杀者做过滤。
        if (combatRoom.RoomType == RoomType.Boss
            && runState.CurrentActIndex == 1
            && !Act4GemManagerPatch.TeamHas<ScoutEnterprise>(runState)
            && KillerTrackerPatch.GetKiller(combatRoom) == player)
        {
            Act4GemManagerPatch.GrantGemReward<ScoutEnterprise>(combatRoom, player, __result);
        }
    }
}
