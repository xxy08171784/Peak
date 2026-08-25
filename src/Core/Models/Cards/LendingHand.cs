using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 伸出援手：1费技能。拥有「被抛弃者」的玩家获得1点费用。
/// </summary>
public sealed class LendingHand : CardModel
{
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/lending_hand.png");

    public LendingHand() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        // 找到有被抛弃者的玩家并给他1费
        var combatState = base.Owner.Creature.CombatState;
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var creature = player.Creature;
            if (creature == null) continue;
            var mark = creature.GetPower<AbandonedMarkPower>();
            if (mark != null && mark.Amount > 0)
            {
                await PlayerCmd.GainEnergy(1m, player);
                break;
            }
        }
    }
}