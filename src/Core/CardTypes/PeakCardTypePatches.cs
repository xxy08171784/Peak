#nullable enable
using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.CardTypes;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Type), MethodType.Getter)]
internal static class PeakCardGameplayTypePatch
{
	[HarmonyPrefix]
	private static bool UseMarkerBackedType(CardModel __instance, ref CardType __result)
	{
		// Panacea implements both interfaces. Food is its primary visible/gameplay type;
		// it remains an IItemCard and still participates in item-specific Peak mechanics.
		if (__instance is IFoodCard)
		{
			__result = PeakCardTypes.RequiredFood;
			return false;
		}

		if (__instance is IItemCard)
		{
			__result = PeakCardTypes.RequiredItem;
			return false;
		}

		return true;
	}
}

[HarmonyPatch(typeof(CardTypeExtensions), nameof(CardTypeExtensions.ToLocString))]
internal static class PeakCardTypeTextPatch
{
	[HarmonyPrefix]
	private static bool UsePeakTypeText(CardType __0, ref LocString __result)
	{
		if (!PeakCardTypes.TryGetVisuals(__0, out PeakCardTypes.TypeVisuals? visuals))
		{
			return true;
		}

		__result = new LocString("gameplay_ui", visuals.LocKey);
		return false;
	}
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Frame), MethodType.Getter)]
internal static class PeakCardFramePatch
{
	[HarmonyPrefix]
	private static bool UsePeakFrame(CardModel __instance, ref Texture2D __result)
	{
		Texture2D? custom = PeakFrames.GetFrame(__instance.Type);
		if (custom is null)
		{
			return true;
		}

		__result = custom;
		return false;
	}
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.PortraitBorder), MethodType.Getter)]
internal static class PeakCardPortraitBorderPatch
{
	[HarmonyPrefix]
	private static bool UsePeakPortraitBorder(CardModel __instance, ref Texture2D __result)
	{
		Texture2D? custom = PeakFrames.GetPortraitBorder(__instance.Type);
		if (custom is null)
		{
			return true;
		}

		__result = custom;
		return false;
	}
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.FrameMaterial), MethodType.Getter)]
internal static class PeakCardFrameMaterialPatch
{
	[HarmonyPrefix]
	private static bool UsePeakFrameMaterial(CardModel __instance, ref Material __result)
	{
		Material? custom = PeakFrames.GetFrameMaterial(__instance.Type);
		if (custom is null)
		{
			return true;
		}

		__result = custom;
		return false;
	}
}

[HarmonyPatch(typeof(CardModel), "GetResultLocationForCardPlay")]
internal static class PeakCardPowerLifecyclePatch
{
	[HarmonyPrefix]
	private static bool PreserveDeclaredPowerLifecycle(CardModel __instance, ref CardLocation __result)
	{
		if (!PeakCardTypes.IsPeakType(__instance.Type)
			|| PeakCardTypes.GetDeclaredBehaviorType(__instance) != CardType.Power)
		{
			return true;
		}

		// These cards still apply their original PowerModel effect exactly once.
		// Returning None preserves the vanilla Power-card removal path even though
		// their public gameplay category is now Item/Food.
		__result = new CardLocation(__instance.Owner, PileType.None, CardPilePosition.Bottom);
		return false;
	}
}
