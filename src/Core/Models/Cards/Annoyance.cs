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

namespace peak.Core.Models.Cards;

/// <summary>
/// 恼怒：2费攻击。造成30点伤害。获得其造成的伤害值一半的格挡。消耗。
/// </summary>
public sealed class Annoyance : CardModel
{
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/annoyance.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(30, ValueProp.Move)
    };

    public Annoyance() : base(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var damage = base.DynamicVars["Damage"].BaseValue;
        await CreatureCmd.Damage(ctx, target, damage, ValueProp.Move, base.Owner.Creature, this, cardPlay);
        // 获得一半伤害的格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, damage / 2m, ValueProp.Move, cardPlay);
    }
}