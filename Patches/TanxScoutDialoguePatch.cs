using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 为 Tanx 先古事件注入 Scout（童子军）专属对话。
/// </summary>
[HarmonyPatch(typeof(Tanx), "DefineDialogues")]
public static class TanxScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		var scoutDialogues = new List<AncientDialogue>
		{
			// 初见（VisitIndex 0）：先古送武器 -> 主角吐槽
			new AncientDialogue("", "")
			{
				VisitIndex = 0
			},
			// 第 2 次见（VisitIndex 1）：主角吐槽 -> 先古再送 -> 主角叹气
			new AncientDialogue("", "", "")
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
