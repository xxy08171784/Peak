#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.CardPools;
using peak.Core.Models.Characters;

namespace peak.Patches;

/// <summary>
/// 卡牌百科（图鉴）的 Scout 支持。
///
/// 原版 NCardLibrary._Ready() 只把 5 个官方角色注册进 _cardPoolFilters 字典，
/// Scout 角色不在其中。当玩家以 Scout 进行过游戏后，OnSubmenuOpened() 里执行
/// _cardPoolFilters[characterModel] 会抛 KeyNotFoundException → 图鉴卡死。
///
/// 本文件包含两个补丁：
///  1. CardLibraryScoutReadyPatch（_Ready Postfix）：动态创建一个 Scout 专属筛选按钮，
///     与铁甲战士/静默猎手的按钮完全一致（使用 character_icon_scout.png 图标），
///     注册进 _poolFilters / _cardPoolFilters，点击即可只看 Scout 卡牌；
///  2. CardLibraryScoutPatch（OnSubmenuOpened Prefix）：进入图鉴前验证 Scout 映射
///     已经存在；若注册链路失效，抛出包含上下文的错误，禁止无内容灰屏静默通过。
/// </summary>

[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
public static class CardLibraryScoutReadyPatch
{
	private const string PoolToggleScenePath = "res://scenes/screens/card_library/library_pool_toggle.tscn";
	private const string ScoutIconPath = "res://images/ui/top_panel/character_icon_scout.png";
	private const string ScoutNodeName = "ScoutPool";
	private const string ScoutPoolLocKey = "POOL_SCOUT_TIP";

	[HarmonyPostfix]
	static void Postfix(NCardLibrary __instance)
	{
		FieldInfo cardPoolFiltersField = AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_cardPoolFilters");
		FieldInfo poolFiltersField = AccessTools.Field(typeof(NCardLibrary), "_poolFilters")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_poolFilters");
		FieldInfo ironcladField = AccessTools.Field(typeof(NCardLibrary), "_ironcladFilter")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_ironcladFilter");

		Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters =
			cardPoolFiltersField.GetValue(__instance) as Dictionary<CharacterModel, NCardPoolFilter>
			?? throw new InvalidDataException("NCardLibrary._cardPoolFilters has an unexpected runtime type.");
		Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters =
			poolFiltersField.GetValue(__instance) as Dictionary<NCardPoolFilter, Func<CardModel, bool>>
			?? throw new InvalidDataException("NCardLibrary._poolFilters has an unexpected runtime type.");
		NCardPoolFilter ironcladFilter = ironcladField.GetValue(__instance) as NCardPoolFilter
			?? throw new InvalidDataException("NCardLibrary._ironcladFilter is missing or has an unexpected runtime type.");

		CharacterModel scoutModel = ModelDb.Character<Scout>();
		if (cardPoolFilters.ContainsKey(scoutModel))
		{
			Log.Info("CardLibraryScoutReadyPatch: Scout pool filter was already registered.");
			return;
		}

		PackedScene poolToggleScene = GD.Load<PackedScene>(PoolToggleScenePath)
			?? throw new FileNotFoundException("Required Scout card-library filter scene is missing.", PoolToggleScenePath);
		Texture2D scoutIcon = GD.Load<Texture2D>(ScoutIconPath)
			?? throw new FileNotFoundException("Required Scout card-library icon is missing.", ScoutIconPath);
		NCardPoolFilter scoutFilter = poolToggleScene.Instantiate<NCardPoolFilter>();
		scoutFilter.Name = ScoutNodeName;

		TextureRect image = scoutFilter.GetNodeOrNull<TextureRect>("Image")
			?? throw new InvalidDataException("Scout card-library filter scene has no Image TextureRect.");
		TextureRect shadow = scoutFilter.GetNodeOrNull<TextureRect>("Image/Shadow")
			?? throw new InvalidDataException("Scout card-library filter scene has no Image/Shadow TextureRect.");
		image.Texture = scoutIcon;
		shadow.Texture = scoutIcon;

		GridContainer poolFiltersContainer = ironcladFilter.GetParent() as GridContainer
			?? throw new InvalidDataException("Ironclad card-library filter parent is not a GridContainer.");
		poolFiltersContainer.AddChild(scoutFilter);

		MethodInfo updateMethod = AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter")
			?? throw new MissingMethodException(typeof(NCardLibrary).FullName, "UpdateCardPoolFilter");
		Callable callable = Callable.From<NCardPoolFilter>(filter =>
			updateMethod.Invoke(__instance, new object[] { filter }));
		scoutFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);

		poolFilters[scoutFilter] = (CardModel c) => c.Pool is ScoutCardPool;
		cardPoolFilters[scoutModel] = scoutFilter;
		scoutFilter.Loc = LocString.GetIfExists("card_library", ScoutPoolLocKey);
		scoutFilter.Visible = true;

		FieldInfo lastHoveredField = AccessTools.Field(typeof(NCardLibrary), "_lastHoveredControl")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_lastHoveredControl");
		scoutFilter.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
			lastHoveredField.SetValue(__instance, scoutFilter)));

		Log.Info($"CardLibraryScoutReadyPatch: Scout pool filter registered; poolCards={ModelDb.CardPool<ScoutCardPool>().AllCards.Count()}.");
	}
}

/// <summary>
/// OnSubmenuOpened 之前验证 Scout 已注册到 _cardPoolFilters。
/// 缺失映射时立即抛错并输出已注册角色，禁止再次静默卡在灰色遮罩。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), "OnSubmenuOpened")]
public static class CardLibraryScoutPatch
{
	[HarmonyPrefix]
	static void Prefix(NCardLibrary __instance)
	{
		FieldInfo runStateField = AccessTools.Field(typeof(NCardLibrary), "_runState")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_runState");
		IRunState? runState = runStateField.GetValue(__instance) as IRunState;
		Player? localPlayer = LocalContext.GetMe(runState);
		CharacterModel? character = localPlayer?.Character;
		if (character is not Scout)
		{
			return;
		}

		FieldInfo cardPoolFiltersField = AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters")
			?? throw new MissingFieldException(typeof(NCardLibrary).FullName, "_cardPoolFilters");
		Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters =
			cardPoolFiltersField.GetValue(__instance) as Dictionary<CharacterModel, NCardPoolFilter>
			?? throw new InvalidDataException("NCardLibrary._cardPoolFilters has an unexpected runtime type.");

		if (!cardPoolFilters.ContainsKey(character))
		{
			throw new InvalidOperationException(
				$"Scout card-library filter was not registered. Registered characters: {string.Join(", ", cardPoolFilters.Keys)}");
		}

		Log.Info("CardLibraryScoutPatch: Scout filter invariant verified before opening the library.");
	}
}
