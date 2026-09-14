using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 恶化：
///   1) 玩家失去生命时，该玩家获得 3 层灾厄（DoomPower）。
///   2) Boss 被攻击时，向攻击者的抽牌堆洗入一张「眩晕」(Dazed)。
/// </summary>
public sealed class WorsenPower : PowerModel
{
    private const decimal DoomOnHpLoss = 3m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (result.UnblockedDamage <= 0)
        {
            return;
        }

        // 1) 玩家失去生命 → 获得 3 层灾厄
        if (target != base.Owner && target.Player != null)
        {
            Flash();
            await PowerCmd.Apply<DoomPower>(choiceContext, target, DoomOnHpLoss, base.Owner, null);
            return;
        }

        // 2) Boss 被攻击 → 给攻击者洗一张眩晕
        if (target == base.Owner && props.IsPoweredAttack())
        {
            Player? attacker = dealer?.Player;
            var combatState = base.Owner.CombatState;
            if (attacker == null || combatState == null)
            {
                return;
            }

            Flash();
            CardModel daze = combatState.CreateCard<Dazed>(attacker);
            await CardPileCmd.AddGeneratedCardToCombat(daze, PileType.Draw, attacker, CardPilePosition.Random);
        }
    }
}
