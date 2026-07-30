using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using peak.Core.Models.CardPools;
using peak.Core.Models.Characters;
using peak.Core.Models.PotionPools;
using peak.Core.Models.RelicsPools; // 确保导入您的 Scout 命名空间

namespace peak.Patches;


[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class ModelDbAllCardPoolsPatch
{
	static void Postfix(ref IEnumerable<CardPoolModel> __result)
	{
		__result = __result
			.Append(ModelDb.CardPool<ScoutCardPool>())
			.Distinct();
	}
}
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
public static class ModelDbAllRelicPoolsPatch
{
	static void Postfix(ref IEnumerable<RelicPoolModel> __result)
	{
		__result = __result
			.Append(ModelDb.RelicPool<ScoutRelicPool>())
			.Distinct();
	}
}
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllPotionPools), MethodType.Getter)]
public static class ModelDbAllPotionPoolsPatch
{
	static void Postfix(ref IEnumerable<PotionPoolModel> __result)
	{
		__result = __result
			.Append(ModelDb.PotionPool<ScoutPotionPool>())
			.Distinct();
	}
}
[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
public static class ModelDbAllCharactersPatch
{
	[HarmonyPostfix]
	static void Postfix(ref IEnumerable<CharacterModel> __result)
	{
		__result = __result
			.Append(ModelDb.Character<Scout>())
			.Distinct();
	}
}
