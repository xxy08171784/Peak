using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 待选卡牌——在玩家回合开始时弹出二选一界面，玩家选完加入手牌后自动移除。
/// Amount=1 → 恼怒/宽恕（T4），Amount=2 → 悲鸣/祈愿（T5/T0）。
/// </summary>
public sealed class PendingCardChoicePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner?.Player) return;
        if (Owner.CombatState == null) return;

        Flash();

        var cs = Owner.CombatState;

        List<CardModel> options;
        if (Amount == 1)
        {
            options = new List<CardModel>
            {
                cs.CreateCard<Annoyance>(player),
                cs.CreateCard<Forgiveness>(player),
            };
        }
        else
        {
            options = new List<CardModel>
            {
                cs.CreateCard<Lament>(player),
                cs.CreateCard<Pray>(player),
            };
        }

        var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: false);
        if (chosen != null)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(new[] { chosen }, PileType.Hand, player);
        }

        await PowerCmd.Remove(this);
    }
}