using System;
using System.Collections.Generic;
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
///  2. CardLibraryScoutPatch（OnSubmenuOpened Prefix）：防御性兜底 —— 万一按钮创建
///     失败，仍把 Scout 映射到 Ironclad 按钮，保证图鉴不会崩溃。
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
		try
		{
			// 1. 读取私有字段
			FieldInfo? cardPoolFiltersField = AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters");
			FieldInfo? poolFiltersField = AccessTools.Field(typeof(NCardLibrary), "_poolFilters");
			FieldInfo? ironcladField = AccessTools.Field(typeof(NCardLibrary), "_ironcladFilter");
			if (cardPoolFiltersField == null || poolFiltersField == null || ironcladField == null)
			{
				return;
			}
			if (cardPoolFiltersField.GetValue(__instance) is not Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters ||
				poolFiltersField.GetValue(__instance) is not Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters)
			{
				return;
			}
			if (ironcladField.GetValue(__instance) is not NCardPoolFilter ironcladFilter)
			{
				return;
			}

			// 已注册过 Scout（防御，正常不会走到）
			CharacterModel scoutModel = ModelDb.Character<Scout>();
			if (cardPoolFilters.ContainsKey(scoutModel))
			{
				return;
			}

			// 2. 实例化筛选按钮场景（与铁甲战士同一模板，自带 Image/Shadow/%SelectionReticle）
			PackedScene? poolToggleScene = GD.Load<PackedScene>(PoolToggleScenePath);
			if (poolToggleScene == null)
			{
				return;
			}
			NCardPoolFilter scoutFilter = poolToggleScene.Instantiate<NCardPoolFilter>();
			scoutFilter.Name = ScoutNodeName;

			// 3. 换成 Scout 图标（Image 及其 Shadow 子节点）
			Texture2D? scoutIcon = GD.Load<Texture2D>(ScoutIconPath);
			if (scoutIcon != null)
			{
				if (scoutFilter.GetNodeOrNull<TextureRect>("Image") is TextureRect image)
				{
					image.Texture = scoutIcon;
				}
				if (scoutFilter.GetNodeOrNull<TextureRect>("Image/Shadow") is TextureRect shadow)
				{
					shadow.Texture = scoutIcon;
				}
			}

			// 4. 加入 PoolFilters 容器（铁甲按钮的父节点）；AddChild 后 _Ready 自动执行
			if (ironcladFilter.GetParent() is not GridContainer poolFiltersContainer)
			{
				scoutFilter.Free();
				return;
			}
			poolFiltersContainer.AddChild(scoutFilter);

			// 5. 连接 Toggled → UpdateCardPoolFilter（私有方法，反射构造 Callable）
			MethodInfo? updateMethod = AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter");
			if (updateMethod != null)
			{
				Callable callable = Callable.From<NCardPoolFilter>(filter =>
					updateMethod.Invoke(__instance, new object[] { filter }));
				scoutFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
			}

			// 6. 注册过滤条件与角色映射
			poolFilters[scoutFilter] = (CardModel c) => c.Pool is ScoutCardPool;
			cardPoolFilters[scoutModel] = scoutFilter;

			// 7. 悬浮提示 + 可见性（Scout 默认解锁）
			scoutFilter.Loc = LocString.GetIfExists("card_library", ScoutPoolLocKey);
			scoutFilter.Visible = true;

			// 8. 焦点进入时记录到 _lastHoveredControl（与其它按钮行为一致）
			FieldInfo? lastHoveredField = AccessTools.Field(typeof(NCardLibrary), "_lastHoveredControl");
			if (lastHoveredField != null)
			{
				scoutFilter.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
					lastHoveredField.SetValue(__instance, scoutFilter)));
			}

			Log.Info("CardLibraryScoutReadyPatch: Scout pool filter button created.");
		}
		catch (Exception e)
		{
			// 创建失败不阻塞图鉴，由 OnSubmenuOpened 的 Prefix 兜底
			Log.Error($"CardLibraryScoutReadyPatch failed: {e}");
		}
	}
}

/// <summary>
/// 防御性兜底：OnSubmenuOpened 之前确保 Scout 已注册到 _cardPoolFilters，
/// 避免 KeyNotFoundException 导致图鉴卡死。
/// 正常情况（Scout 专属按钮已创建）下会直接跳过。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), "OnSubmenuOpened")]
public static class CardLibraryScoutPatch
{
	[HarmonyPrefix]
	static void Prefix(NCardLibrary __instance)
	{
		try
		{
			// 读取私有字段 _runState 以获取当前角色
			FieldInfo? runStateField = AccessTools.Field(typeof(NCardLibrary), "_runState");
			IRunState? runState = runStateField?.GetValue(__instance) as IRunState;

			Player? localPlayer = LocalContext.GetMe(runState);
			CharacterModel? character = localPlayer?.Character;
			if (character == null)
			{
				return; // 主菜单打开时无角色上下文，原逻辑走 Ironclad 分支
			}

			// 读取 _cardPoolFilters 字典
			if (AccessTools.Field(typeof(NCardLibrary), "_cardPoolFilters")?.GetValue(__instance)
				is not Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters)
			{
				return;
			}

			// 已有映射（含 Scout 专属按钮），无需处理
			if (cardPoolFilters.ContainsKey(character))
			{
				return;
			}

			// 兜底：映射到 Ironclad 按钮，并扩展其过滤条件包含 Scout 卡池
			if (AccessTools.Field(typeof(NCardLibrary), "_ironcladFilter")?.GetValue(__instance)
				is not NCardPoolFilter ironcladFilter)
			{
				return;
			}
			cardPoolFilters[character] = ironcladFilter;

			if (AccessTools.Field(typeof(NCardLibrary), "_poolFilters")?.GetValue(__instance)
				is Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters &&
				poolFilters.TryGetValue(ironcladFilter, out Func<CardModel, bool>? existing))
			{
				poolFilters[ironcladFilter] = (CardModel c) => existing(c) || c.Pool is ScoutCardPool;
			}
		}
		catch
		{
			// 兜底失败也不能让图鉴崩溃
		}
	}
}
