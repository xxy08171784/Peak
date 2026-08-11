using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 塞包里：获得 6（9）点格挡，将一张手牌放到抽牌堆顶。
/// 1 费，技能牌，普通稀有度，目标自身。
/// </summary>
public sealed class AllIntoBackpack : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/all_into_backpack.png");

	// 获得格挡
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(6m, ValueProp.Move)
	};

	public AllIntoBackpack()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 6（9）点格挡
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

		// 2. 选择 1 张手牌放到抽牌堆顶
		CardModel? selected = (await CardSelectCmd.FromHand(
			context: choiceContext,
			player: base.Owner,
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
			filter: null,
			source: this)).FirstOrDefault();

		if (selected != null)
		{
			await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后格挡 6 -> 9 (+3)
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}