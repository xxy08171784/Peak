using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using peak.Core.Models.Cards;

namespace peak.Patches;

/// <summary>
/// 给原版 ArchaicTooth 的 TranscendenceUpgrades 映射加入 Scout 的
/// Climb → ReachThePeak（登峰造极），使 Orobas 先古事件中的"古老牙齿"选项
/// 对童子军生效：拾起后把初始牌【攀登】变化为先古牌【登峰造极】。
/// 同时 TranscendenceCards 会包含 ReachThePeak，使 DustyTome 自动排除它，
/// 从而 Darv 事件的"尘封魔典"对童子军会随机到【凌驾】(Override)。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth))]
public static class ArchaicToothTranscendencePatch
{
	[HarmonyPatch("TranscendenceUpgrades", MethodType.Getter)]
	[HarmonyPrefix]
	private static bool TranscendenceUpgradesPrefix(ref Dictionary<ModelId, CardModel> __result)
	{
		// 重新构造完整映射：先包含原版 5 对，再追加 Scout 的 Climb → ReachThePeak
		var upgrades = new Dictionary<ModelId, CardModel>
		{
			{ ModelDb.Card<Bash>().Id, ModelDb.Card<Break>() },
			{ ModelDb.Card<Neutralize>().Id, ModelDb.Card<Suppress>() },
			{ ModelDb.Card<Unleash>().Id, ModelDb.Card<Protector>() },
			{ ModelDb.Card<FallingStar>().Id, ModelDb.Card<MeteorShower>() },
			{ ModelDb.Card<Dualcast>().Id, ModelDb.Card<Quadcast>() },
			{ ModelDb.Card<Climb>().Id, ModelDb.Card<ReachThePeak>() }
		};
		__result = upgrades;
		return false;
	}
}
