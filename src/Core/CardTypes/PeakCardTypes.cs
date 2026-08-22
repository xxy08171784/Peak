#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using BaseLib.Patches.Content;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.CardTypes;

/// <summary>
/// Peak's two gameplay card types. BaseLib assigns collision-resistant enum values
/// before <c>ModelDb.Init</c>; callers use the required properties so an ordering or
/// dependency failure stops model construction instead of silently becoming None.
/// </summary>
public static class PeakCardTypes
{
	[CustomEnum]
	public static CardType Food;

	[CustomEnum]
	public static CardType Item;

	public static CardType RequiredFood => RequireInitialized(Food, nameof(Food));

	public static CardType RequiredItem => RequireInitialized(Item, nameof(Item));

	internal sealed record TypeVisuals(string LocKey, string FrameFileName, string BorderFileName);

	private static Dictionary<CardType, TypeVisuals>? _registry;
	private static readonly FieldInfo DeclaredTypeField =
		AccessTools.Field(typeof(CardModel), "<Type>k__BackingField")
		?? throw new MissingFieldException(typeof(CardModel).FullName, "<Type>k__BackingField");

	internal static IReadOnlyDictionary<CardType, TypeVisuals> Registry
	{
		get
		{
			EnsureInitialized();
			return _registry ??= new Dictionary<CardType, TypeVisuals>
			{
				[Food] = new(
					"PEAK-CARD_TYPE.FOOD",
					"card_frame_food_s.png",
					"card_portrait_border_food_s.png"),
				[Item] = new(
					"PEAK-CARD_TYPE.ITEM",
					"card_frame_item_s.png",
					"card_portrait_border_item_s.png")
			};
		}
	}

	internal static void EnsureInitialized()
	{
		_ = RequiredFood;
		_ = RequiredItem;
		if (Food == Item)
		{
			throw new InvalidOperationException($"Peak custom CardType collision: Food and Item both equal {(int)Food}.");
		}
	}

	internal static bool IsPeakType(CardType type)
	{
		EnsureInitialized();
		return type == Food || type == Item;
	}

	/// <summary>
	/// Returns the vanilla behavior category passed to CardModel's constructor.
	/// The public Type getter is patched to expose Food/Item as the real gameplay
	/// category, while a few engine lifecycle decisions still need the declared
	/// Power category to preserve the card's original one-play behavior.
	/// </summary>
	internal static CardType GetDeclaredBehaviorType(CardModel card)
	{
		object? value = DeclaredTypeField.GetValue(card);
		return value is CardType type
			? type
			: throw new InvalidDataException($"CardModel backing Type is invalid for {card.GetType().FullName}.");
	}

	internal static bool TryGetVisuals(CardType type, [MaybeNullWhen(false)] out TypeVisuals visuals)
	{
		if (IsPeakType(type) && Registry.TryGetValue(type, out TypeVisuals? found))
		{
			visuals = found;
			return true;
		}

		visuals = null;
		return false;
	}

	private static CardType RequireInitialized(CardType value, string name)
	{
		if (value == default)
		{
			throw new InvalidOperationException(
				$"PeakCardTypes.{name} was read before BaseLib assigned [CustomEnum] values. " +
				"Verify that peak.json declares the BaseLib dependency and that BaseLib loads first.");
		}

		return value;
	}
}
