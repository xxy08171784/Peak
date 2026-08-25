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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 悲鸣：1费攻击。造成35点伤害。使这名敌人在本回合失去3点力量。消耗。
/// </summary>
public sealed class Lament : CardModel
{
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/lament.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(35, ValueProp.Move)
    };

    public Lament() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await CreatureCmd.Damage(ctx, target, base.DynamicVars["Damage"].BaseValue, ValueProp.Move, base.Owner.Creature, this, cardPlay);
        // 目标本回合失去3点力量
        await PowerCmd.Apply<StrengthPower>(ctx, target, -3m, base.Owner.Creature, this);
    }
}