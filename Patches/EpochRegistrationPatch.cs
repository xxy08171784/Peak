using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using peak.Core.Timeline.Epochs;
using peak.Core.Timeline.Stories;

namespace peak.Patches;

/// <summary>
/// 补丁 EpochModel.Get(string)，支持 Scout epoch ID 查找。
/// </summary>
[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.Get), new Type[] { typeof(string) })]
public static class EpochModelGetPatch
{
	private static readonly Dictionary<string, Type> _idToType = new()
	{
		["SCOUT1_EPOCH"] = typeof(Scout1Epoch),
		["SCOUT2_EPOCH"] = typeof(Scout2Epoch),
		["SCOUT3_EPOCH"] = typeof(Scout3Epoch),
		["SCOUT4_EPOCH"] = typeof(Scout4Epoch),
		["SCOUT5_EPOCH"] = typeof(Scout5Epoch),
		["SCOUT6_EPOCH"] = typeof(Scout6Epoch),
	};

	static bool Prefix(string id, ref EpochModel __result)
	{
		if (_idToType.TryGetValue(id, out Type? type))
		{
			__result = (EpochModel)Activator.CreateInstance(type);
			return false;
		}
		return true;
	}

	public static Type GetTypeForId(string id) => _idToType[id];
}

/// <summary>
/// 补丁 EpochModel.GetId(Type)，支持 Scout epoch 类型的 ID 查找。
/// </summary>
[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.GetId), new Type[] { typeof(Type) })]
public static class EpochModelGetIdPatch
{
	private static readonly Dictionary<Type, string> _typeToId = new()
	{
		[typeof(Scout1Epoch)] = "SCOUT1_EPOCH",
	[typeof(Scout2Epoch)] = "SCOUT2_EPOCH",
		[typeof(Scout3Epoch)] = "SCOUT3_EPOCH",
		[typeof(Scout4Epoch)] = "SCOUT4_EPOCH",
		[typeof(Scout5Epoch)] = "SCOUT5_EPOCH",
		[typeof(Scout6Epoch)] = "SCOUT6_EPOCH",
	};

	static bool Prefix(Type t, ref string __result)
	{
		if (_typeToId.TryGetValue(t, out string? id))
		{
			__result = id;
			return false;
		}
		return true;
	}

	public static string GetIdForType(Type t) => _typeToId[t];
}

/// <summary>
/// 补丁 StoryModel.Get(string)，支持 Scout 故事查找。
/// </summary>
[HarmonyPatch(typeof(StoryModel), nameof(StoryModel.Get), new Type[] { typeof(string) })]
public static class StoryModelGetPatch
{
	static bool Prefix(string id, ref StoryModel __result)
	{
		if (id.Equals("SCOUT", StringComparison.OrdinalIgnoreCase))
		{
			__result = new ScoutStory();
			return false;
		}
		return true;
	}
}

/// <summary>
/// 补丁 NeowEpoch.QueueUnlocks()，仿 Silent1Epoch 模式：
/// 在首次打开时间线时获得 Scout1Epoch（第一章）。
/// </summary>
[HarmonyPatch(typeof(NeowEpoch), nameof(NeowEpoch.QueueUnlocks))]
public static class NeowEpochQueueUnlocksPatch
{
	static void Postfix()
	{
		SaveManager.Instance.ObtainEpochOverride(
			EpochModel.GetId<Scout1Epoch>(), EpochState.ObtainedNoSlot);
	}
}

/// <summary>
/// 补丁 NeowEpoch.GetTimelineExpansion()，将 Scout1-6 槽位注入初始时间线。
/// Scout1 的槽位和获得分开处理（QueueUnlocks 只获得 Scout1），
/// 槽位由这里注入到 Neow 的展开列表中。
/// </summary>
[HarmonyPatch(typeof(NeowEpoch), nameof(NeowEpoch.GetTimelineExpansion))]
public static class NeowEpochExpansionPatch
{
	static void Postfix(ref EpochModel[] __result)
	{
		var list = new List<EpochModel>(__result);
		list.Add(EpochModel.Get(EpochModel.GetId<Scout1Epoch>()));
		list.Add(EpochModel.Get(EpochModel.GetId<Scout2Epoch>()));
		list.Add(EpochModel.Get(EpochModel.GetId<Scout3Epoch>()));
		list.Add(EpochModel.Get(EpochModel.GetId<Scout4Epoch>()));
		list.Add(EpochModel.Get(EpochModel.GetId<Scout5Epoch>()));
		list.Add(EpochModel.Get(EpochModel.GetId<Scout6Epoch>()));
		__result = list.ToArray();
	}
}

/// <summary>
/// 补丁 EpochModel.IsValid(string)，让游戏识别 Scout epoch 为合法。
/// FilterAndSortEpochs 会调用此方法清洗存档中未注册的 epoch。
/// </summary>
[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.IsValid))]
public static class EpochModelIsValidPatch
{
	static bool Prefix(string id, ref bool __result)
	{
		if (id == "SCOUT1_EPOCH" || id == "SCOUT2_EPOCH" ||
			id == "SCOUT3_EPOCH" || id == "SCOUT4_EPOCH" ||
			id == "SCOUT5_EPOCH" || id == "SCOUT6_EPOCH")
		{
			__result = true;
			return false;
		}
		return true;
	}
}

/// <summary>
/// 将 Scout epoch 注入到所有必要的底层数据结构中。
/// 由 ModInitializer 在 Harmony 补丁加载后调用。
/// </summary>
public static class ScoutEpochRegistrar
{
	private static readonly string[] _scoutEpochIds = new[]
	{
		"SCOUT1_EPOCH", "SCOUT2_EPOCH", "SCOUT3_EPOCH",
		"SCOUT4_EPOCH", "SCOUT5_EPOCH", "SCOUT6_EPOCH"
	};

	private static readonly Type[] _scoutEpochTypes = new[]
	{
		typeof(Scout1Epoch), typeof(Scout2Epoch), typeof(Scout3Epoch),
		typeof(Scout4Epoch), typeof(Scout5Epoch), typeof(Scout6Epoch)
	};

	public static void Register()
	{
		// 1. 注入 _allEpochs 列表
		InjectAllEpochs();

		// 2. 注入 _typeToIdDictionary 和 _epochTypeDictionary
		//    GetId<T>() 直接查这些字典，不走 Harmony 补丁！
		InjectTypeDictionaries();

		// 3. 注入序列化缓存
		InjectSerializationCache();
	}

	private static void InjectAllEpochs()
	{
		var field = typeof(EpochModel).GetField("_allEpochs",
			BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is List<Type> list)
		{
			foreach (var t in _scoutEpochTypes)
			{
				if (!list.Contains(t))
					list.Add(t);
			}

			var cacheField = typeof(EpochModel).GetField("_allEpochIds",
				BindingFlags.NonPublic | BindingFlags.Static);
			cacheField?.SetValue(null, null);
		}
	}

	private static void InjectTypeDictionaries()
	{
		var typeToIdField = typeof(EpochModel).GetField("_typeToIdDictionary",
			BindingFlags.NonPublic | BindingFlags.Static);
		var idToTypeField = typeof(EpochModel).GetField("_epochTypeDictionary",
			BindingFlags.NonPublic | BindingFlags.Static);

		if (typeToIdField?.GetValue(null) is Dictionary<Type, string> typeToId)
		{
			foreach (var t in _scoutEpochTypes)
				typeToId[t] = EpochModelGetIdPatch.GetIdForType(t);
		}

		if (idToTypeField?.GetValue(null) is Dictionary<string, Type> idToType)
		{
			foreach (var id in _scoutEpochIds)
				idToType[id] = EpochModelGetPatch.GetTypeForId(id);
		}
	}

	private static void InjectSerializationCache()
	{
		var cacheType = typeof(ModelIdSerializationCache);

		var nameToNetMap = cacheType.GetField("_epochNameToNetIdMap",
			BindingFlags.NonPublic | BindingFlags.Static);
		var netToNameMap = cacheType.GetField("_netIdToEpochNameMap",
			BindingFlags.NonPublic | BindingFlags.Static);

		if (nameToNetMap?.GetValue(null) is Dictionary<string, int> dict &&
			netToNameMap?.GetValue(null) is List<string> nameList)
		{
			foreach (var id in _scoutEpochIds)
			{
				if (!dict.ContainsKey(id))
				{
					dict[id] = nameList.Count;
					nameList.Add(id);
				}
			}

			var bitSizeField = cacheType.GetProperty("EpochIdBitSize",
				BindingFlags.Public | BindingFlags.Static);
			if (bitSizeField != null)
			{
				int newBitSize = Godot.Mathf.CeilToInt(
					Math.Log2(nameList.Count));
				bitSizeField.SetValue(null, newBitSize);
			}
		}
	}
}
