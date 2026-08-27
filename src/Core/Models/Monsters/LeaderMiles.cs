using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Cards;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Monsters
{
    public sealed class LeaderMiles : MonsterModel
    {
        public override int MinInitialHp => 600;
        public override int MaxInitialHp => 600;

        private bool _phase2Triggered;

    // 二阶段阈值：MaxHp 的 1/3，随多人缩放动态变化
    private decimal Phase2Threshold => Creature.MaxHp / 3m;

    // State references so we can swap FollowUpState at runtime
    private MoveState _p1_T1;
    private MoveState _p1_T2;
    private MoveState _p1_T3;
    private MoveState _p2_T0;
    private MoveState _p2_T4;
    private MoveState _p2_T5;

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            _phase2Triggered = false;

            // =========================================================================
            // PHASE 1 : HP 200 ~ 600
            //   T1 (给标记+塞牌) -> T2 (攻击+格挡) -> T3 (debuff+检查)
            //      有标记 -> 继续 T2 (加深层数)
            //      无标记 -> 回到 T1 (重新撒标记)
            // =========================================================================

            _p1_T1 = new MoveState("P1_T1", async (targets) =>
            {
                if (await TryTriggerPhaseTransition(targets)) return;

                var ctx = new ThrowingPlayerChoiceContext();
                var players = GetPlayerList(targets);
                int playerCount = players.Count;
                var rng = GetRng(targets);
                int markIdx = rng.NextInt(players.Count);
                var marked = players[markIdx];

                // Mark amount depends on player count
                int markAmt = playerCount switch { 1 => 2, 2 => 3, 3 => 4, _ => 6 };
                await PowerCmd.Apply<AbandonedMarkPower>(ctx, marked.Creature, markAmt, Creature, null);

                // 坚定信念 -> 标记者
                var fb = CreateCard<FirmBelief>(marked, targets);
                await CardPileCmd.AddGeneratedCardsToCombat(new[] { fb }, PileType.Hand, marked);

                // 决不放弃 -> 其他玩家
                foreach (var p in players)
                {
                    if (p == marked) continue;
                    var ngu = CreateCard<NeverGiveUp>(p, targets);
                    await CardPileCmd.AddGeneratedCardsToCombat(new[] { ngu }, PileType.Hand, p);
                }
            }, new DebuffIntent())
            {
                FollowUpStateId = "P1_T2"
            };

            _p1_T2 = new MoveState("P1_T2", async (targets) =>
            {
                if (await TryTriggerPhaseTransition(targets)) return;

                var ctx = new ThrowingPlayerChoiceContext();

                // 攻击 12×2
                foreach (var t in targets)
                {
                    await CreatureCmd.Damage(ctx, t, 12m, ValueProp.Move, Creature, null, null);
                    await CreatureCmd.Damage(ctx, t, 12m, ValueProp.Move, Creature, null, null);
                }

                // 30 × 玩家人数 格挡
                decimal block = 30m * targets.Count();
                await CreatureCmd.GainBlock(Creature, block, ValueProp.Move, null);
            }, new MultiAttackIntent(12, 2))
            {
                FollowUpStateId = "P1_T3"
            };

            _p1_T3 = new MoveState("P1_T3", async (targets) =>
            {
                if (await TryTriggerPhaseTransition(targets)) return;

                var ctx = new ThrowingPlayerChoiceContext();
                var players = GetPlayerList(targets);
                int playerCount = players.Count;

                // 全员 1 层虚弱、脆弱、易伤
                foreach (var p in players)
                {
                    await PowerCmd.Apply<WeakPower>(ctx, p.Creature, 1m, Creature, null);
                    await PowerCmd.Apply<VulnerablePower>(ctx, p.Creature, 1m, Creature, null);
                    await PowerCmd.Apply<FrailPower>(ctx, p.Creature, 1m, Creature, null);
                }

                // 检查是否有玩家仍有被抛弃者
                bool hasActiveMark = players.Any(p =>
                    (p.Creature.GetPowerAmount<AbandonedMarkPower>()) > 0);

                if (hasActiveMark)
                {
                    int deepenAmt = playerCount switch { 1 => 2, 2 => 3, 3 => 4, _ => 6 };

                    foreach (var p in players)
                    {
                        var mark = p.Creature.GetPower<AbandonedMarkPower>();
                        if (mark != null && mark.Amount > 0)
                        {
                            // 加深层数（同一个人继续被抛弃，不再重复给坚定信念）
                            await PowerCmd.ModifyAmount(ctx, mark, deepenAmt, Creature, null);
                        }
                        else
                        {
                            // 给其他人决不放弃
                            var ngu = CreateCard<NeverGiveUp>(p, targets);
                            await CardPileCmd.AddGeneratedCardsToCombat(new[] { ngu }, PileType.Hand, p);
                        }
                    }

                    // 继续 T2（不撒新标记）
                    _p1_T3.FollowUpState = _p1_T2;
                }
                else
                {
                    // 无标记 → 回 T1 重新标记
                    _p1_T3.FollowUpState = _p1_T1;
                }
            }, new DebuffIntent());

            // =========================================================================
            // PHASE 2 : HP 0 ~ 200
            //   T0 (阶段转换首动：回复生命值到 1/3 + 全员3层易伤 + 延迟：悲鸣/祈愿二选一) -> T4
            //   T4 (6×5 攻击 + 获得孤独 + 延迟：恼怒/宽恕二选一) -> T5
            //   T5 (35 攻击 + 移除孤独 + 延迟：悲鸣/祈愿二选一) -> T4
            //   孤独跨 T4~T5 持续，T5 攻击后移除
            //   所有选牌延迟到玩家回合开始时弹出，玩家看清意图后再选
            // =========================================================================

            _p2_T0 = new MoveState("P2_T0", async (targets) =>
            {
                var players = GetPlayerList(targets);

                // 回复生命值到 1/3（MaxHp 的 1/3 = 200）
                decimal thirdHp = Creature.MaxHp / 3m;
                decimal healAmount = thirdHp - Creature.CurrentHp;
                if (healAmount > 0m)
                {
                    await CreatureCmd.Heal(Creature, healAmount);
                }

                // 给所有玩家 3 层易伤
                var vulnerableCtx = new ThrowingPlayerChoiceContext();
                foreach (var p in players)
                {
                    await PowerCmd.Apply<VulnerablePower>(vulnerableCtx, p.Creature, 3m, Creature, null);
                }

                // 延迟到玩家回合：二选一悲鸣/祈愿
                foreach (var p in players)
                {
                    await PowerCmd.Apply<PendingCardChoicePower>(
                        new ThrowingPlayerChoiceContext(), p.Creature, 2m, Creature, null);
                }
            }, new HealIntent(), new DebuffIntent())
            {
                // 必须执行一次后才能转走，保证 SetMoveImmediate 后下一动确实是 T0（意图与实际一致）
                MustPerformOnceBeforeTransitioning = true,
                FollowUpStateId = "P2_T4"
            };

            _p2_T4 = new MoveState("P2_T4", async (targets) =>
            {
                // 6×5 攻击（此时无孤独效果）
                foreach (var t in targets)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), t, 6m, ValueProp.Move, Creature, null, null);
                    }
                }

                // 获得孤独 Power（持续到 T5 攻击完后手动移除）
                await PowerCmd.Apply<LonelinessPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);

                // 延迟到玩家回合：二选一恼怒/宽恕
                foreach (var t in targets)
                {
                    var player = t.Player;
                    if (player == null) continue;
                    await PowerCmd.Apply<PendingCardChoicePower>(
                        new ThrowingPlayerChoiceContext(), player.Creature, 1m, Creature, null);
                }

            }, new MultiAttackIntent(6, 5))
            {
                FollowUpStateId = "P2_T5"
            };

            _p2_T5 = new MoveState("P2_T5", async (targets) =>
            {
                // 35 攻击（孤独生效中：玩家反击时每次-2临时力量）
                foreach (var t in targets)
                {
                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), t, 35m, ValueProp.Move, Creature, null, null);
                }

                // 移除孤独 Power（持续了T4末尾→T5整回合，用完消失）
                var loneliness = Creature.GetPower<LonelinessPower>();
                if (loneliness != null)
                {
                    await PowerCmd.Remove(loneliness);
                }

                // 延迟到玩家回合：二选一悲鸣/祈愿
                foreach (var t in targets)
                {
                    var player = t.Player;
                    if (player == null) continue;
                    await PowerCmd.Apply<PendingCardChoicePower>(
                        new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
                }
            }, new SingleAttackIntent(35))
            {
                FollowUpStateId = "P2_T4"
            };

            // 初始状态：Phase 1, T1
            return new MonsterMoveStateMachine(
                new List<MonsterState> { _p1_T1, _p1_T2, _p1_T3, _p2_T0, _p2_T4, _p2_T5 },
                _p1_T1
            );
        }

        // =========================================================================
        // Phase Transition — Boss HP ≤ MaxHp/3 (1/3) 时触发
        // 触发时机：玩家回合内 Boss 受到伤害导致 HP ≤ MaxHp/3 的瞬间（AfterDamageReceived），
        //           以及 Boss 行动前的兜底检查（TryTriggerPhaseTransition）。
        // =========================================================================
        private async Task<bool> TryTriggerPhaseTransition(IReadOnlyList<Creature> targets)
        {
            if (_phase2Triggered || Creature.CurrentHp > Phase2Threshold)
                return false;

            await DoPhaseTransition(targets);
            return true;
        }

        /// <summary>
        /// Boss 血量降到 1/3（≤MaxHp/3）后，立即在玩家回合内执行阶段转换（仅第一次触发）：
        ///   1) Boss 获得「领队意志」标记 buff + 临时 1 层「硬化外壳」(HardenedShellPower) ——
        ///      本回合最多承受 1 点伤害，表现上接近无敌；Boss 回合开始时会被移除（下回合失去）；
        ///   2) 清除所有玩家的「被抛弃者」全部层数；
        ///   3) 意图立即切换：回复生命值到 1/3（Heal 图标）+ 施加负面效果（Debuff 图标）：
        ///      给所有玩家 3 层易伤 + 塞一张「悲鸣」、一张「祈愿」到所有玩家抽牌堆顶（下一抽即悲鸣/祈愿）。
        /// </summary>
        private async Task DoPhaseTransition(IReadOnlyList<Creature> targets)
        {
            _phase2Triggered = true;

            var ctx = new ThrowingPlayerChoiceContext();
            var boss = Creature;
            var players = GetPlayerList(targets);

            // 领队意志：进入第二阶段的标记 buff（可视化）
            await PowerCmd.Apply<LeaderWillPower>(ctx, boss, 1m, boss, null);

            // 临时 1 层「硬化外壳」(HardenedShellPower)：本回合最多承受 1 点伤害，近似无敌。
            // 在 Boss 的回合开始时移除（见 BeforeSideTurnStart），实现"临时，下回合失去"。
            await PowerCmd.Apply<HardenedShellPower>(ctx, boss, 1m, boss, null);

            // 清除所有玩家的「被抛弃者」全部层数
            foreach (var p in players)
            {
                var mark = p.Creature.GetPower<AbandonedMarkPower>();
                if (mark != null && mark.Amount > 0)
                {
                    // 层数直接归零（走 ModifyAmount → 自然移除，不再有补丁拦截问题）
                    await PowerCmd.ModifyAmount(ctx, mark, -mark.Amount, boss, null);
                }
            }

            // Boss 获得 希望 Power（初始 0 层，后续由卡牌增加）
            var existingHope = boss.GetPower<HopePower>();
            if (existingHope == null || existingHope.Amount <= 0)
            {
                await PowerCmd.Apply<HopePower>(ctx, boss, 0m, boss, null);
            }

            // 锁定 Phase 2 循环
            _p2_T4.FollowUpState = _p2_T5;
            _p2_T5.FollowUpState = _p2_T4;

            // 切换意图：下一动 = 全员3层易伤 + 塞悲鸣/祈愿到抽牌堆顶（P2_T0）
            Creature.Monster.SetMoveImmediate(_p2_T0, forceTransition: true);
        }

        /// <summary>
        /// Boss 受到伤害后立即检查：HP ≤ MaxHp/3 则立刻进入阶段 2（在玩家回合内生效，
        /// 而不是等到 Boss 回合才开始）。
        /// </summary>
        public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Creature && !_phase2Triggered && Creature.CurrentHp <= Phase2Threshold)
            {
                var targets = Creature.CombatState?.PlayerCreatures ?? new List<Creature>();
                return DoPhaseTransition(targets);
            }
            return base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
        }

        /// <summary>
        /// 硬化外壳是临时的：在 Boss 的回合开始时移除它（下回合失去）。
        /// 这样硬化外壳只保护触发转换后的玩家剩余回合，Boss 行动时（P2_T0 回复+易伤）已不再无敌。
        /// </summary>
        public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == CombatSide.Enemy && participants.Contains(Creature))
            {
                var shell = Creature.GetPower<HardenedShellPower>();
                if (shell != null)
                {
                    // 用 RunSafely 同步移除，避免在同步 hook 里等待异步
                    TaskHelper.RunSafely(PowerCmd.Remove(shell));
                }
            }
            return base.BeforeSideTurnStart(choiceContext, side, participants, combatState);
        }

        /// <summary>
        /// 防止 Boss 在 1/3 以上被单次大额伤害直接击杀：若本次伤害会将其 HP 打到 ≤0，
        /// 在伤害结算前提前触发阶段转换（Boss 获得 Buffer 后该次伤害被抵挡）。
        /// </summary>
        public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Creature && !_phase2Triggered && Creature.CurrentHp - amount <= 0)
            {
                var targets = Creature.CombatState?.PlayerCreatures ?? new List<Creature>();
                return DoPhaseTransition(targets);
            }
            return base.BeforeDamageReceived(choiceContext, target, amount, props, dealer, cardSource);
        }

        // =========================================================================
        // Helpers
        // =========================================================================
        private static List<Player> GetPlayerList(IReadOnlyList<Creature> targets)
        {
            return targets.Select(t => t.Player).Where(p => p != null).ToList();
        }

        private Rng GetRng(IReadOnlyList<Creature> targets)
        {
            var cs = targets.FirstOrDefault()?.CombatState;
            return cs?.RunState.Rng.MonsterAi ?? new Rng(0, "leader_miles_fallback");
        }

        private TCard CreateCard<TCard>(Player owner, IReadOnlyList<Creature> targets)
            where TCard : CardModel
        {
            var cs = targets.FirstOrDefault()?.CombatState;
            if (cs != null)
                return cs.CreateCard<TCard>(owner);
            // fallback — shouldn't happen at runtime
            return (TCard)ModelDb.Card<TCard>().MutableClone();
        }
    }
}
