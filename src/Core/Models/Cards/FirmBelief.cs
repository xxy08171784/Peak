using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 坚定信念：2费技能。失去1层「被抛弃者」。保留。打出后返回手牌。
/// </summary>
public sealed class FirmBelief : CardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/firm_belief.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

    public FirmBelief() : base(2, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        var mark = base.Owner.Creature.GetPower<AbandonedMarkPower>();
        if (mark != null && mark.Amount > 0)
        {
            await PowerCmd.ModifyAmount(ctx, mark, -1m, base.Owner.Creature, this);
        }
        // 返回手牌
        await CardPileCmd.Add(this, PileType.Hand);
    }
}