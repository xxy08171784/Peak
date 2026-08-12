using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 为 Pael 先古事件注入 Scout（童子军）专属对话。
/// </summary>
[HarmonyPatch(typeof(Pael), "DefineDialogues")]
public static class PaelScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		var scoutDialogues = new List<AncientDialogue>
		{
			// 初见（VisitIndex 0）：主角问 -> 先古答 -> 主角赞
			new AncientDialogue("", "", "")
			{
				VisitIndex = 0
			},
			// 后续（VisitIndex 1）：先古问候 -> 主角答
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
