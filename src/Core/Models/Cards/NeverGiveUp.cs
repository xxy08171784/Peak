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
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 决不放弃：0费技能。从三张卡中选择1张加入手牌。虚无。消耗。
/// 三张卡：坚实后盾、伸出援手、相予砥砺
/// </summary>
public sealed class NeverGiveUp : CardModel
{
    public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/never_give_up.png");
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    public NeverGiveUp() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        // 三选一加入手牌
        // 注意：不能直接 new 卡牌（会触发 DuplicateModelException），
        // 必须从 ModelDb 获取 canonical 实例，再通过 CombatState.CreateCard 创建战斗副本。
        var options = new List<CardModel>
        {
            base.CombatState.CreateCard(ModelDb.Card<SolidBacking>(), base.Owner),
            base.CombatState.CreateCard(ModelDb.Card<LendingHand>(), base.Owner),
            base.CombatState.CreateCard(ModelDb.Card<MutualEncouragement>(), base.Owner),
        };

        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, options, base.Owner, canSkip: false);
        if (chosen != null)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(new[] { chosen }, PileType.Hand, base.Owner);
        }
    }
}