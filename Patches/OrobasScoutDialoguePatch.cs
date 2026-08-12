using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 为 Orobas 先古事件注入 Scout（童子军）专属对话。
/// 原版没有 Scout 的对话定义，会导致童子军进入后 GetValidDialogues 找不到对话而卡死。
/// </summary>
[HarmonyPatch(typeof(Orobas), "DefineDialogues")]
public static class OrobasScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		var scoutDialogues = new List<AncientDialogue>
		{
			// 初见（VisitIndex 0）：先古问 -> 主角答 -> 先古送
			new AncientDialogue("", "", "")
			{
				VisitIndex = 0
			},
			// 第 2 次见（VisitIndex 1）：先古问塔顶 -> 主角讲述见闻
			new AncientDialogue("", "")
			{
				VisitIndex = 1
			}
		};

		ScoutAncientDialogueHelper.InjectScoutDialogues(__result, scoutDialogues);
	}
}
