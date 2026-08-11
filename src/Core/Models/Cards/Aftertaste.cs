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
/// 回味：从消耗牌堆中选择一张消耗的食物牌加入你的手牌。
/// 1 费（升级后 0 费），技能牌，稀有稀有度，目标自身，消耗。
/// </summary>
public sealed class Aftertaste : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：aftertaste.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/aftertaste.png");
	

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public Aftertaste()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 从消耗牌堆中选择一张食物牌加入手牌
		CardModel food = (await CardSelectCmd.FromCombatPile(
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
			context: choiceContext,
			pile: PileType.Exhaust.GetPile(base.Owner),
			player: base.Owner,
			filter: (CardModel c) => c is IFoodCard)).FirstOrDefault();

		if (food != null)
		{
			await CardPileCmd.Add(food, PileType.Hand);
		}
	}
	
	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
