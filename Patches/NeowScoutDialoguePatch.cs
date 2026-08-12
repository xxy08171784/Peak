using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 为 Neow 先古事件注入 Scout（童子军）专属对话。
/// 原版没有 Scout 的对话定义，会导致童子军进入后 GetValidDialogues 找不到对话而卡死。
/// </summary>
[HarmonyPatch(typeof(Neow), "DefineDialogues")]
public static class NeowScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		var scoutDialogues = new List<AncientDialogue>
		{
			// 初见（VisitIndex 0）：主角问 -> 先古答
			new AncientDialogue("", "")
			{
				VisitIndex = 0
			},
			// 第 2 次见（VisitIndex 1）：主角抱怨 -> 先古安慰
			new AncientDialogue("", "")
			{
				VisitIndex = 1
			},
			// 败北后再见（VisitIndex 4）：主角抱怨 -> 先古安慰
			new AncientDialogue("", "")
			{
				VisitIndex = 4
			}
		};

		ScoutAncientDialogueHelper.InjectScoutDialogues(__result, scoutDialogues);
	}
}
