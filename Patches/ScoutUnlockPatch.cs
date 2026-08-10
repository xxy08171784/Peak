using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Entities.Players;
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

/// <summary>
/// 跳过 Scout 角色的"打 Boss 解锁 Epoch"逻辑。
/// Scout 是 Mod 自定义角色，没有注册 SCOUT2_EPOCH / SCOUT3_EPOCH / SCOUT4_EPOCH，
/// 原版 ObtainCharUnlockEpoch 会因 EpochModel.Get 找不到 ID 而抛异常，
/// 中断战斗胜利后的奖励流程（无法获得奖励、无法继续）。
/// 这里在 Prefix 中直接拦截并跳过，让 UpdateAfterCombatWon 继续执行后续奖励逻辑。
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
public static class ObtainCharUnlockEpochPatch
{
	[HarmonyPrefix]
	static bool Prefix(Player localPlayer)
	{
		// 只跳过 Scout（通过角色 ID 判断，避免类型耦合）
		if (localPlayer.Character.Id.Entry.Equals("scout", StringComparison.OrdinalIgnoreCase))
		{
			return false; // 跳过原方法
		}
		return true;
	}
}

/// <summary>
/// 跳过 Scout 角色的"检查击败15个精英的 Epoch"逻辑。
/// 原版方法通过 if-else 链判断 character is Ironclad/Silent/Regent/Defect/Necrobinder/Deprived，
/// Scout 不匹配任何类型，直接 throw ArgumentOutOfRangeException 中断流程。
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
public static class CheckFifteenElitesDefeatedEpochPatch
{
	[HarmonyPrefix]
	static bool Prefix(Player localPlayer)
	{
		if (localPlayer.Character.Id.Entry.Equals("scout", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}
}

/// <summary>
/// 跳过 Scout 角色的"检查击败15个 Boss 的 Epoch"逻辑，原因同上。
/// 原版方法同样是 if-else 类型判断链，Scout 会抛 ArgumentOutOfRangeException。
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
public static class CheckFifteenBossesDefeatedEpochPatch
{
	[HarmonyPrefix]
	static bool Prefix(Player localPlayer)
	{
		if (localPlayer.Character.Id.Entry.Equals("scout", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}
}
