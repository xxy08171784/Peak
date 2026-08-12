using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Logging;
using peak.Core.Models.CardPools;
using peak.Core.Models.Characters;
using peak.Core.Models.PotionPools;
using peak.Core.Models.RelicsPools;
using peak.Core.Timeline.Epochs;

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
/// Scout2/3/4 的解锁由原版 ObtainCharUnlockEpoch 通过命名约定自动处理：
///   Act1 Boss → SCOUT2_EPOCH
///   Act2 Boss → SCOUT3_EPOCH
///   Act3 Boss → SCOUT4_EPOCH
/// 无需额外补丁——EpochRegistrationPatch 中的 EpochModel.Get(string) 补丁
/// 已确保游戏能查找到这些 Scout epoch。
/// </summary>

/// <summary>
/// 为 Scout 角色处理"击败 15 个精英"的 Epoch 检测。
/// 原版 CheckFifteenElitesDefeatedEpoch 使用硬编码类型检查链
/// （if character is Ironclad/else if Silent/...），Scout 不在其中会抛异常。
/// 此补丁在 Scout 情况下自行计数并获取 Scout5Epoch。
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
public static class CheckFifteenElitesDefeatedEpochPatch
{
	private static MethodInfo? _getEliteEncountersMethod;
	private static MethodInfo? _tryObtainEpochMidRunMethod;

	[HarmonyPrefix]
	static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
	{
		if (!(localPlayer.Character is Scout))
		{
			return true; // 非 Scout 角色走原版逻辑
		}

		try
		{
			// 通过反射获取精英遭遇集合
			_getEliteEncountersMethod ??= typeof(ProgressSaveManager)
				.GetMethod("GetEliteEncounters", BindingFlags.NonPublic | BindingFlags.Static);
			var eliteEncounters = (HashSet<ModelId>)_getEliteEncountersMethod!.Invoke(null, null)!;

			// 统计 Scout 击败的精英数量
			int count = 0;
			foreach (var stats in __instance.Progress.EncounterStats.Values)
			{
				if (!eliteEncounters.Contains(stats.Id)) continue;
				foreach (var fight in stats.FightStats)
				{
					if (fight.Character == localPlayer.Character.Id)
					{
						count += fight.Wins;
						break;
					}
				}
			}

			if (count >= 15)
			{
				var epoch = EpochModel.Get(EpochModel.GetId<Scout5Epoch>());
				_tryObtainEpochMidRunMethod ??= typeof(ProgressSaveManager)
					.GetMethod("TryObtainEpochMidRun", BindingFlags.NonPublic | BindingFlags.Instance);
				_tryObtainEpochMidRunMethod!.Invoke(__instance, new object[] { epoch, localPlayer });
			}
		}
		catch (Exception ex)
		{
			Log.Error($"[Scout] CheckFifteenElitesDefeatedEpoch failed: {ex}");
		}

		return false; // 跳过原版方法
	}
}

/// <summary>
/// 为 Scout 角色处理"击败 15 个 Boss"的 Epoch 检测。
/// 原版 CheckFifteenBossesDefeatedEpoch 同样使用硬编码类型检查链，
/// Scout 不在其中会抛异常。此补丁在 Scout 情况下自行计数并获取 Scout6Epoch。
/// </summary>
[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
public static class CheckFifteenBossesDefeatedEpochPatch
{
	private static MethodInfo? _tryObtainEpochMidRunMethod;

	[HarmonyPrefix]
	static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
	{
		if (!(localPlayer.Character is Scout))
		{
			return true; // 非 Scout 角色走原版逻辑
		}

		try
		{
			// 收集所有 Boss 遭遇 ID
			var bossIds = ModelDb.Acts
				.SelectMany(a => a.AllBossEncounters.Select(e => e.Id))
				.ToHashSet();

			// 统计 Scout 击败的 Boss 数量
			int count = 0;
			foreach (var stats in __instance.Progress.EncounterStats.Values)
			{
				if (!bossIds.Contains(stats.Id)) continue;
				foreach (var fight in stats.FightStats)
				{
					if (fight.Character == localPlayer.Character.Id)
					{
						count += fight.Wins;
						break;
					}
				}
			}

			if (count >= 15)
			{
				var epoch = EpochModel.Get(EpochModel.GetId<Scout6Epoch>());
				_tryObtainEpochMidRunMethod ??= typeof(ProgressSaveManager)
					.GetMethod("TryObtainEpochMidRun", BindingFlags.NonPublic | BindingFlags.Instance);
				_tryObtainEpochMidRunMethod!.Invoke(__instance, new object[] { epoch, localPlayer });
			}
		}
		catch (Exception ex)
		{
			Log.Error($"[Scout] CheckFifteenBossesDefeatedEpoch failed: {ex}");
		}

		return false; // 跳过原版方法
	}
}
