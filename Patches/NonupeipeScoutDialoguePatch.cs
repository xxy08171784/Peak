using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 为 Nonupeipe 先古事件注入 Scout（童子军）专属对话。
/// </summary>
[HarmonyPatch(typeof(Nonupeipe), "DefineDialogues")]
public static class NonupeipeScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		var scoutDialogues = new List<AncientDialogue>
		{
			// 初见（VisitIndex 0）：先古嫌弃 -> 主角答 -> 先古送衣
			new AncientDialogue("", "", "")
			{
				VisitIndex = 0
			},
			// 第 2 次见（VisitIndex 1）：先古问 -> 主角答
			new AncientDialogue("", "")
			{
				VisitIndex = 1
			},
			// 多次拜访（VisitIndex 4）
			new AncientDialogue("")
			{
				VisitIndex = 4
			}
		};

		ScoutAncientDialogueHelper.InjectScoutDialogues(__result, scoutDialogues);
	}
}
