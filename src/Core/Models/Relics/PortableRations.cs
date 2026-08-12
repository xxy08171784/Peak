using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Relics;

/// <summary>
/// 便携口粮：在战斗开始时，将抽牌堆中的两张随机食物卡放到手牌中。
/// 稀有稀有度。
/// </summary>
public sealed class PortableRations : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		// 从抽牌堆中随机选 2 张食物卡
		List<CardModel> foodCards = PileType.Draw
			.GetPile(base.Owner)
			.Cards
			.Where(c => c is IFoodCard)
			.ToList()
			.StableShuffle(base.Owner.RunState.Rng.CombatCardSelection)
			.Take(2)
			.ToList();

		if (foodCards.Count == 0)
		{
			return;
		}

		Flash();

		foreach (CardModel card in foodCards)
		{
			await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom);
		}
	}
}
