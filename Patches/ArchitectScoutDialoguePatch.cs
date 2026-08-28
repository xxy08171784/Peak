using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;
using Godot;

namespace peak.Patches;

/// <summary>
/// 为 TheArchitect 先古事件注入 Scout（童子军）专属对话。
/// Architect 的 DefineDialogues 是 private static，用 MethodType.Normal 打补丁。
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
public static class ArchitectScoutDialoguePatch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		GD.Print("[ArchitectScoutDialoguePatch] Postfix called! Injecting Scout dialogues for Architect...");

		var scoutDialogues = new List<AncientDialogue>
		{
			// 第 1 次（VisitIndex 0）：主角挡路 -> 先古罚
			new AncientDialogue("", "")
			{
				VisitIndex = 0,
				EndAttackers = ArchitectAttackers.Both
			},
			// 第 2 次（VisitIndex 1）：主角求送 -> 先古拒绝
			new AncientDialogue("", "")
			{
				VisitIndex = 1,
				EndAttackers = ArchitectAttackers.Both
			},
			// 第 3 次（VisitIndex 2）：先古问累不累
			new AncientDialogue("")
			{
				VisitIndex = 2,
				EndAttackers = ArchitectAttackers.Both
			},
			// 第 4 次及以后（VisitIndex 3+）：主角想回家 -> 先古怪航空公司
			// 设置 IsRepeating=true 使高胜场数时也能匹配到这段对话
			new AncientDialogue("", "")
			{
				VisitIndex = 3,
				IsRepeating = true,
				EndAttackers = ArchitectAttackers.Both
			}
		};

		ScoutAncientDialogueHelper.InjectScoutDialogues(__result, scoutDialogues);
		GD.Print("[ArchitectScoutDialoguePatch] Postfix completed.");
	}
}
