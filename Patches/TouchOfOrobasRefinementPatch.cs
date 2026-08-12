using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using peak.Core.Models.Relics;

namespace peak.Patches;

/// <summary>
/// 给原版 TouchOfOrobas（欧洛巴斯之触）的 RefinementUpgrades 映射加入 Scout 的
/// MyClimbing → MountaineerExpert（我的攀登 → 登山专家）。
/// 使 Orobas 先古事件中的"欧洛巴斯之触"选项对童子军生效：
/// 拾起后把初始遗物【我的攀登】替换为强化版【登山专家】。
/// 若不加入此映射，Scout 的初始遗物会退化为头环（Circlet）。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas))]
public static class TouchOfOrobasRefinementPatch
{
	[HarmonyPatch("RefinementUpgrades", MethodType.Getter)]
	[HarmonyPrefix]
	private static bool RefinementUpgradesPrefix(ref Dictionary<ModelId, RelicModel> __result)
	{
		// 重新构造完整映射：先包含原版 5 对，再追加 Scout 的 MyClimbing → MountaineerExpert
		var upgrades = new Dictionary<ModelId, RelicModel>
		{
			{ ModelDb.Relic<BurningBlood>().Id, ModelDb.Relic<BlackBlood>() },
			{ ModelDb.Relic<RingOfTheSnake>().Id, ModelDb.Relic<RingOfTheDrake>() },
			{ ModelDb.Relic<DivineRight>().Id, ModelDb.Relic<DivineDestiny>() },
			{ ModelDb.Relic<BoundPhylactery>().Id, ModelDb.Relic<PhylacteryUnbound>() },
			{ ModelDb.Relic<CrackedCore>().Id, ModelDb.Relic<InfusedCore>() },
			{ ModelDb.Relic<MyClimbing>().Id, ModelDb.Relic<MountaineerExpert>() }
		};
		__result = upgrades;
		return false;
	}
}
