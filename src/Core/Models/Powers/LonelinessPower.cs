using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 孤独 —— Boss 身上的标记 Power。
/// 效果：Boss 每次受到玩家攻击时，失去 2 点临时力量（AttackPower 降幅，回合结束恢复）。
/// 此标记由 T4 行动结束时施加，在 T5 行动结束后手动移除（跨回合持续）。
/// </summary>
public sealed class LonelinessPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    /// <summary>
    /// 每段玩家伤害结算后：Boss 失去 2 点临时力量。
    /// </summary>
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 检查伤害来源是否是玩家，目标是 Boss 自己
        if (target == Owner && dealer != null && dealer.IsPlayer && !result.WasFullyBlocked)
        {
            Flash();
            // 应用临时力量下降（回合结束时自动恢复）
            await PowerCmd.Apply<LonelinessHitStrengthDownPower>(choiceContext, Owner, 2m, Owner, null);
        }
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
    }

    /// <summary>
    /// 孤独不再自动清除——由 P2_T4 施加，P2_T5 攻击完后手动移除。
    /// 临时力量下降（LonelinessHitStrengthDownPower）会在各回合结束时自动恢复。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 不再自动移除；由 LeaderMiles 在 T5 行动结束时手动移除
        await base.AfterSideTurnEnd(choiceContext, side, participants);
    }
}