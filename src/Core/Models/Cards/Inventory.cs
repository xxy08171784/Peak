using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 物品栏：抽 1 张牌，选择 1 张手牌添加保留效果。
/// 0 费，技能牌，普通稀有度，目标自身，消耗（升级后去掉消耗）。
/// </summary>
public sealed class Inventory : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/inventory.png");

	// 固有关键词：消耗（升级后通过 OnUpgrade 移除）
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Inventory()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 抽 1 张牌
		await CardPileCmd.Draw(choiceContext, 1m, base.Owner);

		// 2. 选择 1 张手牌添加保留效果
		CardModel? selected = (await CardSelectCmd.FromHand(
			context: choiceContext,
			player: base.Owner,
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
			filter: c => !c.Keywords.Contains(CardKeyword.Retain),
			source: this)).FirstOrDefault();

		if (selected != null)
		{
			CardCmd.ApplyKeyword(selected, CardKeyword.Retain);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后移除消耗词条
		RemoveKeyword(CardKeyword.Exhaust);
	}
}