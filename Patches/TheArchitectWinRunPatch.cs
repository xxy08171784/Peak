using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace peak.Patches;

/// <summary>
/// 修复自定义角色（Scout）击败三层 Boss 见到建筑师后无法胜利结算的问题。
///
/// 根因：TheArchitect.DefineDialogues() 只为原版 5 个角色（Ironclad/Silent/Defect/
/// Necrobinder/Regent）定义了 CharacterDialogues，AgnosticDialogues 为空，
/// FirstVisitEverDialogue 为 null。自定义角色 Scout 在 LoadDialogue() 时
/// GetValidDialogues 返回空列表，导致 Dialogue == null。点击 PROCEED 触发
/// WinRun() 时，第一行访问 Dialogue.EndAttackers 抛出 NullReferenceException，
/// 胜利结算流程中断。
///
/// 这里在 WinRun 的 Prefix 中兜底：若 Dialogue 仍为 null，注入一个单行无文本的
/// AncientDialogue（默认 EndAttackers=None，攻击动画方法直接返回，无副作用），
/// 让原方法可以正常走到 RunManager.ActChangeSynchronizer.SetLocalPlayerReady()，
/// 完成胜利结算。
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "WinRun")]
public static class TheArchitectWinRunPatch
{
	[HarmonyPrefix]
	static void Prefix(TheArchitect __instance)
	{
		var dialogueField = AccessTools.Field(typeof(TheArchitect), "_dialogue");
		if (dialogueField == null)
		{
			return;
		}

		AncientDialogue? dialogue = dialogueField.GetValue(__instance) as AncientDialogue;
		if (dialogue == null)
		{
			// 单行、无 SFX、无文本的占位对话；EndAttackers 默认 None（安全）
			dialogueField.SetValue(__instance, new AncientDialogue(""));
			GD.Print("[TheArchitectWinRunPatch] Injected default dialogue for custom character.");
		}
	}
}
