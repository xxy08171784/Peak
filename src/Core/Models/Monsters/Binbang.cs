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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;
using peak.Core.Visuals;

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
///
/// 立绘：八张静态场景 binbang1~8 与形态一一对应（常态 / 常态倒地 / 黄金 / 黄金倒地 /
/// 黑化飞行 / 黑化落地 / 黑化倒地 / 疯狂蓄力），在死亡、复活、失去飞行三处整场景切换。
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
    private const decimal DiveDamage = 33m;

    private int _phase = PhaseNormal;
    private bool _awaitingRespawn;
    private bool _finalDeath;
    private bool _diveTriggered;

    private MoveState _deadState;
    private MoveState _diveState;
    private MoveState _destroyState;

    public override int MinInitialHp => NormalHp;

    public override int MaxInitialHp => NormalHp;

    // —— 八张形态立绘（SceneHelper 的内层路径），音效仍复用领队迈尔斯的 ——
    private const string VisualNormal = "creature_visuals/binbang1";     // 常态宾邦
    private const string VisualNormalDead = "creature_visuals/binbang2"; // 常态被打空、倒地等复活
    private const string VisualGolden = "creature_visuals/binbang3";     // 黄金宾邦
    private const string VisualGoldenDead = "creature_visuals/binbang4"; // 黄金被打空、倒地等复活
    private const string VisualDarkFlying = "creature_visuals/binbang5"; // 黑化宾邦（飞行中）
    private const string VisualDarkLanded = "creature_visuals/binbang6"; // 黑化宾邦（失去飞行、落地）
    private const string VisualDarkCorpse = "creature_visuals/binbang7"; // 黑化被打空、倒地等复活
    private const string VisualCharging = "creature_visuals/binbang8";   // 疯狂形态（蓄力准备「销毁」）

    /// <summary>开场立绘：常态宾邦。之后的形态/倒地切换见 <see cref="SwapVisuals"/>。</summary>
    protected override string VisualsPath => SceneHelper.GetScenePath(VisualNormal);

    /// <summary>
    /// 八张立绘全部报给预加载：EncounterModel.GetAssetPaths 会收集每个怪物的 AssetPaths，
    /// 预先算进来就不会在每次变身那一刻现加载造成卡顿。
    /// </summary>
    public override IEnumerable<string> AssetPaths
    {
        get
        {
            foreach (string path in base.AssetPaths)
            {
                yield return path;
            }

            yield return SceneHelper.GetScenePath(VisualNormalDead);
            yield return SceneHelper.GetScenePath(VisualGolden);
            yield return SceneHelper.GetScenePath(VisualGoldenDead);
            yield return SceneHelper.GetScenePath(VisualDarkFlying);
            yield return SceneHelper.GetScenePath(VisualDarkLanded);
            yield return SceneHelper.GetScenePath(VisualDarkCorpse);
            yield return SceneHelper.GetScenePath(VisualCharging);
        }
    }

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

        // 开局给每名玩家挂一个「混沌」实例（写法参考原版天青玻璃 Aeonglass 施加凋萎存在）：
        // 混沌是 InstanceType.Instanced，每个实例用 Target 认领一名玩家、各自倒数 3 张牌。
        // 实例都挂在宾邦身上，但 PowerModel.IsVisible 只让玩家看见 Target 是自己那一个，
        // 所以每人屏幕上只有一个混沌图标、显示自己的倒计时。
        foreach (Creature player in combatState.PlayerCreatures)
        {
            ChaosPower chaos = (ChaosPower)ModelDb.Power<ChaosPower>().ToMutable();
            chaos.Target = player;
            await PowerCmd.Apply(Ctx(), chaos, Creature, 1m, Creature, null);
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        // wasRemovalPrevented 指的是"死亡被 ShouldDie 挡下"（不是"移除被阻止"）。
        // 这种情况下宾邦根本没死，绝不能进倒地/复活流程——否则 _awaitingRespawn=true
        // 会让它活着却打不到（ShouldAllowHitting 返回 false），还会白涨一格 _phase。
        // 同原版实验体的 AdaptablePower.AfterDeath 判断。
        if (creature != Creature || _finalDeath || wasRemovalPrevented)
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

        // 倒地立绘：_phase 此时还没自增，正好是"刚被打空的那个形态"
        SwapVisuals(_phase switch
        {
            PhaseNormal => VisualNormalDead,
            PhaseGolden => VisualGoldenDead,
            _ => VisualDarkCorpse,
        });

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

        // 失去飞行 → 落地立绘
        SwapVisuals(VisualDarkLanded);

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

        // 复活同时换上新形态的立绘：黄金 / 黑化飞行 / 疯狂（蓄力）
        SwapVisuals(_phase switch
        {
            PhaseGolden => VisualGolden,
            PhaseDark => VisualDarkFlying,
            _ => VisualCharging,
        });

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

    /// <summary>
    /// 复活并回满血。
    ///
    /// 血量必须按多人缩放后再写：第 1 条命的 333 是引擎在建怪时缩放的
    /// （CombatState.CreateCreature → Creature.ScaleMonsterHpForMultiplayer），
    /// 复活这里如果直接写死 222 / 444，多人局里第 2、3 条命就会变成单人数值、越打越软。
    /// 写法照抄原版实验体 TestSubject.Revive。
    /// </summary>
    private async Task Revive(int baseRespawnHp)
    {
        decimal hp = baseRespawnHp;

        var combatState = CombatState;
        if (combatState != null)
        {
            hp = MegaCrit.Sts2.Core.Entities.Creatures.Creature.ScaleHpForMultiplayer(
                baseRespawnHp,
                combatState.Encounter,
                combatState.Players.Count,
                combatState.RunState.CurrentActIndex);
        }

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
        }, new SingleAttackIntent(21));

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
        }, new SingleAttackIntent(27));

        ritual9.FollowUpState = attack27;
        attack27.FollowUpState = attack27;

        // —— 形态 3：黑化宾邦（飞行中）——
        var flyAttack = new MoveState("BB_FLY_2X15", async (targets) =>
        {
            var ctx = Ctx();
            foreach (Creature target in targets)
            {
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
                await CreatureCmd.Damage(ctx, target, 2m, ValueProp.Move, Creature);
            }
        }, new MultiAttackIntent(2, 15));

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
                await CreatureCmd.Damage(ctx, target, DiveDamage, ValueProp.Move, Creature);
                await PowerCmd.Apply<WeakPower>(ctx, target, 3m, Creature, null);
            }
        }, new SingleAttackIntent(() => DiveDamage), new DebuffIntent());

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

        // 终止招式也得有后继：MoveState.GetNextState 在 FollowUpState 和 FollowUpStateId
        // 都为 null 时会抛 InvalidOperationException("No valid followup state.")。
        // 正常流程里打完「销毁」宾邦就死了并被移出战斗，轮不到下一回合的 RollMove；
        // 这里自指做兜底，写法同原版瀑布巨人的 EXPLODE_MOVE（WaterfallGiant.cs:213）。
        _destroyState.FollowUpState = _destroyState;

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
  
    // ================= 立绘切换 =================

    /// <summary>
    /// 把宾邦的外观整场景换成指定立绘（实现见 <see cref="CreatureVisualSwapper"/>）。
    /// 换的场景都是纯静态 Sprite2D，所以不会影响招式逻辑；找不到节点或替换失败时只留日志。
    /// </summary>
    private void SwapVisuals(string innerScenePath)
    {
        NCreature creatureNode = CreatureVisualSwapper.FindCreatureNode(Creature);
        if (creatureNode == null)
        {
            Godot.GD.PushWarning($"[Binbang] 找不到宾邦的 NCreature 节点，跳过换成 {innerScenePath}。");
            return;
        }

        CreatureVisualSwapper.Swap(creatureNode, innerScenePath);
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
