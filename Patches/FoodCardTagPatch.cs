using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using peak.Core.Models.Cards;

namespace peak.Patches;

/// <summary>
/// 为 IFoodCard 卡牌添加"食物"标签，并在图鉴库中添加"食物"搜索关键词。
/// </summary>

[HarmonyPatch]
public static class FoodCardTagPatch
{
	/// <summary>
	/// 在描述文字后追加"食物"标签行
	/// </summary>
	[HarmonyPatch(typeof(CardModel), "GetDescriptionForPile", new Type[] { typeof(PileType), typeof(Creature) })]
	[HarmonyPostfix]
	static void AddFoodTagToDescription(CardModel __instance, ref string __result)
	{
		// 只对 IFoodCard 生效
		if (__instance is not IFoodCard)
			return;

		// 在末尾加上"食物"标签
		__result += "\n[purple]食物。[/purple]";
	}

	/// <summary>
	/// 在图鉴库 _Ready 中注册"食物"搜索关键词
	/// </summary>
	[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
	[HarmonyPostfix]
	static void AddFoodKeywords(NCardLibrary __instance)
	{
		try
		{
			FieldInfo? specialKeywordsField = AccessTools.Field(typeof(NCardLibrary), "_specialSearchbarKeywords");
			if (specialKeywordsField != null && specialKeywordsField.GetValue(__instance) is Dictionary<string, Func<CardModel, bool>> keywords)
			{
				keywords["食物"] = (CardModel c) => c is IFoodCard;
			}
			Log.Info("[FoodCardTagPatch] 已注册食物标签和搜索关键词");
		}
		catch (System.Exception ex)
		{
			Log.Error($"[FoodCardTagPatch] 注册失败: {ex}");
		}
	}
}