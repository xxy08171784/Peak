using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Relics;

/// <summary>
/// 营养金字塔：每打出一张食物卡回复 1 点生命值。
/// 稀有稀有度。
/// </summary>
public sealed class NutritionPyramid : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && cardPlay.Card is IFoodCard && base.Owner?.Creature != null)
		{
			Flash();
			await CreatureCmd.Heal(base.Owner.Creature, 1m);
		}
	}
}
