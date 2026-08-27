using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 坚实后盾：1费能力。每当你在回合中获得格挡时，拥有「被抛弃者」的玩家获得其一半格挡。
/// </summary>
public sealed class SolidBacking : CardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/solid_backing.png");

    public SolidBacking() : base(1, CardType.Power, CardRarity.Token, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        // 应用坚实后盾能力：每当你在回合中获得格挡时，被抛弃者玩家获得其一半格挡
        await PowerCmd.Apply<SolidBackingPower>(ctx, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}