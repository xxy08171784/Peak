#nullable enable
using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.CardTypes;

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
internal static class PeakCardTypeAuditPatch
{
	[HarmonyPostfix]
	private static void VerifyRegisteredTypes()
	{
		PeakCardTypes.EnsureInitialized();

		CardModel[] markedCards = ModelDb.AllCards
			.OfType<CardModel>()
			.Where(card => card is IFoodCard || card is IItemCard)
			.ToArray();
		if (markedCards.Length == 0)
		{
			throw new InvalidOperationException("Peak custom card type audit found zero IFoodCard/IItemCard models.");
		}

		foreach (CardModel card in markedCards)
		{
			CardType expected = card is IFoodCard ? PeakCardTypes.Food : PeakCardTypes.Item;
			if (card.Type != expected)
			{
				throw new InvalidOperationException(
					$"Peak card type invariant failed: {card.Id} expected {(int)expected}, actual {(int)card.Type}.");
			}
		}

		int foodCount = markedCards.Count(card => card is IFoodCard);
		int itemOnlyCount = markedCards.Count(card => card is IItemCard && card is not IFoodCard);
		int dualCount = markedCards.Count(card => card is IFoodCard && card is IItemCard);
		int declaredPowerCount = markedCards.Count(card =>
			PeakCardTypes.GetDeclaredBehaviorType(card) == CardType.Power);

		Log.Info(
			$"Peak custom card types verified: Food={(int)PeakCardTypes.Food}, " +
			$"Item={(int)PeakCardTypes.Item}, marked={markedCards.Length}, " +
			$"food={foodCount}, itemOnly={itemOnlyCount}, dual={dualCount}, " +
			$"declaredPowerLifecycle={declaredPowerCount}.");
	}
}
