using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 宽恕：2费技能。获得16点格挡。获得2点希望。消耗。
/// </summary>
public sealed class Forgiveness : CardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/forgiveness.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(16, ValueProp.Move)
    };

    public Forgiveness() : base(2, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars["Block"].BaseValue, ValueProp.Move, cardPlay);
        // 给敌人（Boss）加2层希望
        var boss = base.Owner.Creature.CombatState?.GetOpponentsOf(base.Owner.Creature).FirstOrDefault();
        if (boss != null)
        {
            await PowerCmd.Apply<HopePower>(ctx, boss, 2m, base.Owner.Creature, this);
        }
    }
}