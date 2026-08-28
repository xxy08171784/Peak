using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

public sealed class LateAtNightPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 可堆叠层数
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 在玩家的回合结束时，给予所有敌人 1 层寒冷。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 检查是否是本能力持有者的回合结束
        if (!participants.Contains(base.Owner))
        {
            return;
        }

        Flash();

        decimal coldAmount = base.Amount;
        var combatState = base.Owner.CombatState;
        if (combatState == null) return;

        // 给予所有敌人寒冷
        foreach (var enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<ColdPower>(choiceContext, enemy, coldAmount, base.Owner, null);
        }
    }
}