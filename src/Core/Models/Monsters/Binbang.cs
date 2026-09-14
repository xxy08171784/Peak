using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Monsters;

/// <summary>
/// 隐藏 Boss「宾邦」——三命形态（写法参考原版实验体 TestSubject）：
///
///   1. 常态宾邦 333：开局获得「混沌」（每打出 3 张牌触发一次随机效果）。
///       状态机 T1 攻击21 → T2 防御32 → T3 强化(触发3次混沌) 循环。
///   2. 黄金宾邦 222：复活时获得 88 层硬化外壳（每回合吸收 88 点伤害）。
///       状态机 T1 仪式9（仅一次）→ T2 攻击27 循环。
///   3. 黑化宾邦 444：复活时获得「飞行」（减伤减半，被攻击 6/12/18/24 次后失去并俯冲）+「insane」。
///       飞行中：T1 2×15 → T2 +2力量 循环；失去飞行 → 俯冲(33 + 全体3虚弱) → T1 诅咒(一次) → T2/T3 循环。
///   疯狂：黑化死亡时把血条变为 9999999、意图「销毁」——对所有玩家造成 999 可格挡伤害，然后真正死亡。
///
/// 多命实现（同实验体）：通过 Should* 钩子阻止移除/结束战斗/被攻击，Boss 以 0 血进入"死亡状态"，
/// 轮到自己时执行复活招式（RespawnMove）切形态。
/// </summary>
public sealed class Binbang : MonsterModel
{
    private const int PhaseNormal = 0;
    private const int PhaseGolden = 1;
    private const int PhaseDark = 2;
    private const int PhaseInsane = 3;

    public const int NormalHp = 333;
    public const int GoldenHp = 222;
    public const int DarkHp = 444;

    private const decimal DestroyDamage = 999m;

    private int _phase = PhaseNormal;
    private bool _awaitingRespawn;
    private bool _finalDeath;
    private bool _diveTriggered;

    private MoveState _deadState;
    private MoveState _diveState;
    private MoveState _destroyState;

    public override int MinInitialHp => NormalHp;

    public override int MaxInitialHp => NormalHp;

    // —— 复用领队迈尔斯的视觉场景与音效（不新增美术资源）——
    protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/leader_miles");

    protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/leader_miles/leader_miles_attack";

    protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/leader_miles/leader_miles_cast";

    public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/leader_miles/leader_miles_die";

    // ================= 三命机制（实验体式） =================

    /// <summary>疯狂状态被「销毁」之前，宾邦不会被移出战斗。</summary>
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
        => creature != Creature || _finalDeath;

    /// <summary>疯狂状态之前，战斗不会因宾邦死亡而结束。</summary>
    public override bool ShouldStopCombatFromEnding() => !_finalDeath;

    /// <summary>0 血等待复活期间不可被攻击（同实验体）。</summary>
    public override bool ShouldAllowHitting(Creature creature)
        => creature != Creature || !_awaitingRespawn;

    /// <summary>前两条命对"灾厄即死"免疫。</summary>
    public override bool ShouldDisappearFromDoom => _phase >= PhaseDark;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        // 入口：所有玩家回复"已损失生命"的 80%
        foreach (Creature player in combatState.PlayerCreatures)
        {
            decimal lostHp = player.MaxHp - player.CurrentHp;
            if (lostHp > 0m)
            {
                await CreatureCmd.Heal(player, lostHp * 0.8m);
            }
        }

        // 开局获得「混沌」
        await PowerCmd.Apply<ChaosPower>(Ctx(), Creature, 1m, Creature, null);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Creature || _finalDeath)
        {
            return;
        }

        await TriggerDeadState();
    }

    /// <summary>
    /// 死亡但未到最终阶段：播死亡动画并切到"死亡状态"，等宾邦轮到自己执行复活招式。
    /// </summary>
    private async Task TriggerDeadState()
    {
        _awaitingRespawn = true;
        await CreatureCmd.TriggerAnim(Creature, "Dead", 0f);
        SetMoveImmediate(_deadState, forceTransition: true);
    }

    /// <summary>由 <see cref="BinbangFlightPower"/> 调用：失去飞行 → 切到俯冲意图（只触发一次）。</summary>
    public void EnterDive()
    {
        if (_diveTriggered || CombatState == null || _finalDeath)
        {
            return;
        }

        _diveTriggered = true;
        SetMoveImmediate(_diveState, forceTransition: true);
    }

    /// <summary>
    /// 复活招式：切到下一形态。第 3 次"复活"= 疯狂形态（血条 9999999 + 意图销毁）。
    /// 之后由 ConditionalBranchState 选择下一形态的首个招式（或销毁状态）。
    /// </summary>
    private async Task RespawnMove(IReadOnlyList<Creature> targets)
    {
        _awaitingRespawn = false;
        _phase++;

        switch (_phase)
        {
            case PhaseGolden:
                await Revive(GoldenHp);
                // 硬化外壳：每回合吸收 88 点伤害（原版 HardenedShellPower 会在回合开始重置计数）
                await PowerCmd.Apply<HardenedShellPower>(Ctx(), Creature, 88m, Creature, null);
                break;

            case PhaseDark:
                await Revive(DarkHp);
                // 飞行（每名玩家 6 次 → 6/12/18/24）+ insane
                _diveTriggered = false;
                await PowerCmd.Apply<BinbangFlightPower>(Ctx(), Creature, 6m, Creature, null);
                await PowerCmd.Apply<InsanePower>(Ctx(), Creature, 1m, Creature, null);
                break;

            default: // PhaseInsane：血条变成 9999999，意图由分支切到「销毁」
                await CreatureCmd.SetMaxHp(Creature, 9999999m);
                await CreatureCmd.Heal(Creature, 9999999m);
                Creature.HpDisplay = HpDisplay.InfiniteWithNumbers;
                break;
        }
    }

    private async Task Revive(int hp)
    {
        await CreatureCmd.SetMaxHp(Creature, hp);
        await CreatureCmd.Heal(Creature, hp);
    }

    /// <summary>销毁：所有玩家失去 999 生命（可格挡），随后宾邦真正死亡。</summary>
    private async Task DestroyMove(IReadOnlyList<Creature> targets)
    {
        _finalDeath = true;
        await DamageCmd.Attack(DestroyDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.1f)
            .Execute(null);
        await CreatureCmd.Kill(Creature);
    }

    // ================= 状态机 =================

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // —— 形态 1：常态宾邦 ——
        var attack21 = new MoveState("BB_ATTACK_21", async (targets) =>
        {
            await AttackAll(targets, 21m);
        }, new MultiAttackIntent(21, 1));

        var defend32 = new MoveState("BB_DEFEND_32", async (targets) =>
        {
            await CreatureCmd.GainBlock(Creature, 32m, ValueProp.Move, null);
        }, new DefendIntent());

        var chaos = new MoveState("BB_CHAOS", async (targets) =>
        {
            if (Creature.GetPower<ChaosPower>() is ChaosPower power)
            {
                await power.TriggerRandomEffects(3);
            }
        }, new BuffIntent());

        attack21.FollowUpState = defend32;
        defend32.FollowUpState = chaos;
        chaos.FollowUpState = attack21;

        // —— 形态 2：黄金宾邦 ——
        var ritual9 = new MoveState("BB_RITUAL_9", async (targets) =>
        {
            await PowerCmd.Apply<RitualPower>(Ctx(), Creature, 9m, Creature, null);
        }, new BuffIntent());

        var attack27 = new MoveState("BB_ATTACK_27", async (targets) =>
        {
            await AttackAll(targets, 27m);
        }, new MultiAttackIntent(27, 1));

        ritual9.FollowUpState = attack27;
        attack27.FollowUpState = attack27;

        // —— 形态 3：黑化宾邦（飞行中）——
        var flyAttack = new MoveState("BB_FLY_2X15", async (targets) =>
        {
            var ctx = Ctx();
            foreach (Creature target in targets)
            {
                await CreatureCmd.Damage(ctx, target, 15m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 15m, ValueProp.Move, Creature);
            }
        }, new MultiAttackIntent(15, 2));

        var flyBuff = new MoveState("BB_FLY_BUFF_2", async (targets) =>
        {
            await PowerCmd.Apply<StrengthPower>(Ctx(), Creature, 2m, Creature, null);
        }, new BuffIntent());

        flyAttack.FollowUpState = flyBuff;
        flyBuff.FollowUpState = flyAttack;

        // —— 俯冲（失去飞行后）——
        _diveState = new MoveState("BB_DIVE_33", async (targets) =>
        {
            // 兜底：确认飞行已移除（参考猫头鹰法官的俯冲招式 VerdictMove 会显式移除飞行 power）
            if (Creature.GetPower<BinbangFlightPower>() != null)
            {
                await PowerCmd.Remove<BinbangFlightPower>(Creature);
            }

            var ctx = Ctx();
            foreach (Creature target in targets)
            {
                await CreatureCmd.Damage(ctx, target, 33m, ValueProp.Move, Creature);
                await PowerCmd.Apply<WeakPower>(ctx, target, 3m, Creature, null);
            }
        }, new MultiAttackIntent(33, 1), new DebuffIntent());

        // —— 落地后：T1 诅咒（一次）→ T2/T3 循环 ——
        var curse = new MoveState("BB_CURSE", async (targets) =>
        {
            await PowerCmd.Apply<CursePower>(Ctx(), Creature, 1m, Creature, null);
        }, new BuffIntent());

        var worsenDefend = new MoveState("BB_WORSEN_27", async (targets) =>
        {
            var ctx = Ctx();
            await PowerCmd.Apply<WorsenPower>(ctx, Creature, 1m, Creature, null);
            await CreatureCmd.GainBlock(Creature, 27m, ValueProp.Move, null);
        }, new BuffIntent(), new DefendIntent());

        var attack6x6 = new MoveState("BB_ATTACK_6X6", async (targets) =>
        {
            var ctx = Ctx();
            foreach (Creature target in targets)
            {
                for (int i = 0; i < 6; i++)
                {
                    await CreatureCmd.Damage(ctx, target, 6m, ValueProp.Move, Creature);
                }
            }

            await PowerCmd.Apply<StrengthPower>(ctx, Creature, 2m, Creature, null);
        }, new MultiAttackIntent(6, 6), new BuffIntent());

        _diveState.FollowUpState = curse;
        curse.FollowUpState = worsenDefend;
        worsenDefend.FollowUpState = attack6x6;
        attack6x6.FollowUpState = worsenDefend;

        // —— 死亡 / 复活 / 销毁 ——
        _deadState = new MoveState("BB_RESPAWN", RespawnMove, new HealIntent(), new BuffIntent())
        {
            MustPerformOnceBeforeTransitioning = true
        };

        _destroyState = new MoveState("BB_DESTROY", DestroyMove, new DeathBlowIntent(() => DestroyDamage));

        var branch = new ConditionalBranchState("BB_REVIVE_BRANCH");
        branch.AddState(ritual9, () => _phase == PhaseGolden);
        branch.AddState(flyAttack, () => _phase == PhaseDark);
        branch.AddState(_destroyState, () => _phase >= PhaseInsane);
        _deadState.FollowUpState = branch;

        return new MonsterMoveStateMachine(
            new List<MonsterState>
            {
                attack21, defend32, chaos,
                ritual9, attack27,
                flyAttack, flyBuff, _diveState, curse, worsenDefend, attack6x6,
                _deadState, _destroyState, branch,
            },
            attack21);
    }

    private async Task AttackAll(IReadOnlyList<Creature> targets, decimal damage)
    {
        var ctx = Ctx();
        foreach (Creature target in targets)
        {
            await CreatureCmd.Damage(ctx, target, damage, ValueProp.Move, Creature);
        }
    }

    private static ThrowingPlayerChoiceContext Ctx() => new ThrowingPlayerChoiceContext();
}
