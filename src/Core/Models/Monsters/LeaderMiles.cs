using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

        // State references so we can swap FollowUpState at runtime
        private MoveState _p1_T1;
        private MoveState _p1_T2;
        private MoveState _p1_T3;
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
            }, new SingleAttackIntent(24))
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
                            // 加深层数
                            await PowerCmd.ModifyAmount(ctx, mark, deepenAmt, Creature, null);

                            // 给标记者坚定信念
                            var fb = CreateCard<FirmBelief>(p, targets);
                            await CardPileCmd.AddGeneratedCardsToCombat(new[] { fb }, PileType.Hand, p);
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
            //   T4 (6×5 攻击 + Annoyance+Forgiveness + Loneliness)
            //   T5 (35 攻击 + Lament+Lament)
            //   T4 ↔ T5 交替
            // =========================================================================

            _p2_T4 = new MoveState("P2_T4", async (targets) =>
            {
                var ctx = new ThrowingPlayerChoiceContext();

                // 6×5 攻击（每段触发孤独 -2 力量）
                foreach (var t in targets)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        await CreatureCmd.Damage(ctx, t, 6m, ValueProp.Move, Creature, null, null);
                        await LonelinessPower.OnBossHit(Creature, ctx);
                    }
                }

                // 塞恼怒 + 宽恕
                foreach (var t in targets)
                {
                    var player = t.Player;
                    if (player == null) continue;
                    var annoy = CreateCard<Annoyance>(player, targets);
                    var forgive = CreateCard<Forgiveness>(player, targets);
                    await CardPileCmd.AddGeneratedCardsToCombat(new CardModel[] { annoy, forgive }, PileType.Hand, player);
                }

                // 获得孤独 Power
                await PowerCmd.Apply<LonelinessPower>(ctx, Creature, 1m, Creature, null);
            }, new MultiAttackIntent(6, 5))
            {
                FollowUpStateId = "P2_T5"
            };

            _p2_T5 = new MoveState("P2_T5", async (targets) =>
            {
                var ctx = new ThrowingPlayerChoiceContext();

                // 35 攻击
                foreach (var t in targets)
                {
                    await CreatureCmd.Damage(ctx, t, 35m, ValueProp.Move, Creature, null, null);
                }

                // 塞悲鸣 + 悲鸣
                foreach (var t in targets)
                {
                    var player = t.Player;
                    if (player == null) continue;
                    var lam1 = CreateCard<Lament>(player, targets);
                    var lam2 = CreateCard<Lament>(player, targets);
                    await CardPileCmd.AddGeneratedCardsToCombat(new CardModel[] { lam1, lam2 }, PileType.Hand, player);
                }
            }, new SingleAttackIntent(35))
            {
                FollowUpStateId = "P2_T4"
            };

            // 初始状态：Phase 1, T1
            return new MonsterMoveStateMachine(
                new List<MonsterState> { _p1_T1, _p1_T2, _p1_T3, _p2_T4, _p2_T5 },
                _p1_T1
            );
        }

        // =========================================================================
        // Phase Transition — HP < 200 triggers LeaderWillPower
        // =========================================================================
        private async Task<bool> TryTriggerPhaseTransition(IReadOnlyList<Creature> targets)
        {
            if (_phase2Triggered || Creature.CurrentHp > 200)
                return false;

            _phase2Triggered = true;

            var ctx = new ThrowingPlayerChoiceContext();
            var boss = Creature;
            var players = targets.Select(t => t.Player).Where(p => p != null).ToList();

            // 本回合免疫伤害 — 用 Buffer 9 层让本回合免疫
            await PowerCmd.Apply<BufferPower>(ctx, boss, 9m, boss, null);

            // 清除所有玩家的被抛弃者
            foreach (var p in players)
            {
                var mark = p.Creature.GetPower<AbandonedMarkPower>();
                if (mark != null && mark.Amount > 0)
                {
                    // 直接设 0 层（不走 PowerCmd.Remove 避免被补丁拦截）
                    await PowerCmd.ModifyAmount(ctx, mark, -mark.Amount, boss, null);
                }
                // 全员 3 层易伤
                await PowerCmd.Apply<VulnerablePower>(ctx, p.Creature, 3m, boss, null);
            }

            // 塞悲鸣 + 祈愿到抽牌堆顶（这里直接用 Hand，近似效果）
            foreach (var p in players)
            {
                var lament = CreateCard<Lament>(p, targets);
                var pray = CreateCard<Pray>(p, targets);
                await CardPileCmd.AddGeneratedCardsToCombat(new CardModel[] { lament, pray }, PileType.Hand, p);
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

            // 下回合立即切到 T4
            Creature.Monster.SetMoveImmediate(_p2_T4, forceTransition: true);

            return true;
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
