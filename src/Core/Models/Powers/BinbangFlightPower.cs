using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Monsters;

namespace peak.Core.Models.Powers;

/// <summary>
/// 宾邦的「飞行」：受到伤害减半；每被（有力量的）攻击命中一次减 1 层，
/// 层数归零后失去飞行并进入俯冲意图。
///
/// 减半与"按攻击次数递减"参考原版偷窃草蜢的 FlutterPower；
/// 不同点：失去飞行后不是被击晕，而是切到宾邦的俯冲招式。
/// 层数基础值 6，ShouldScaleInMultiplayer 让它随人数变成 6/12/18/24。
/// </summary>
public sealed class BinbangFlightPower : PowerModel
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(DamageDecreaseKey, 50m)
    };

    /// <summary>
    /// 层数 = 每名玩家 6 次，即单人 6 / 双人 12 / 三人 18 / 四人 24。
    /// （默认实现还会额外乘一个遭遇难度系数，得不到规格里的确切数字，所以这里直接按人数算。）
    /// </summary>
    public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
    {
        return amount * combatState.Players.Count;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return base.DynamicVars[DamageDecreaseKey].BaseValue / 100m;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || result.UnblockedDamage == 0 || !props.IsPoweredAttack())
        {
            return;
        }

        await PowerCmd.Decrement(this);

        if (base.Amount > 0)
        {
            return;
        }

        // 失去飞行 → 进入俯冲（层数归零会被自动移除，无需再 Remove）
        Flash();
        if (base.Owner.Monster is Binbang binbang)
        {
            binbang.EnterDive();
        }
    }
}
