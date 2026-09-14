using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 诅咒：玩家打出以下卡牌时，受到 10 点伤害：
/// 友谊喇叭 / 万灵药 / 诅咒头骨 / 仙子提灯 / 潘多拉餐盒 / 骸骨之书 / 领队喇叭。
/// </summary>
public sealed class CursePower : PowerModel
{
    private const decimal PunishDamage = 10m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? player = cardPlay.Card.Owner;
        Creature? playerCreature = player?.Creature;
        if (playerCreature == null || playerCreature == base.Owner)
        {
            return;
        }

        if (!IsCursedCard(cardPlay.Card))
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(choiceContext, playerCreature, PunishDamage,
            ValueProp.Unpowered | ValueProp.Move, base.Owner, null, null);
    }

    private static bool IsCursedCard(CardModel card) =>
        card is FriendshipHorn
            or Panacea
            or CursedSkull
            or TheHandLanternOfFaery
            or PandoraBox
            or TheBookOfBones
            or LeaderTrumpet;
}
