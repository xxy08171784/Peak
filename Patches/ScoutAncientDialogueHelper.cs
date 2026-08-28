using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using peak.Core.Models.Characters;

namespace peak.Patches;

/// <summary>
/// 给先古事件的 DefineDialogues() 返回值注入 Scout 角色的专属对话。
/// 原版只定义了 IRONCLAD/SILENT/DEFECT/NECROBINDER/REGENT 五个角色的对话，
/// 没有 Scout，导致童子军进入先古事件时 GetValidDialogues 找不到对话而卡死。
/// 通过反射替换 AncientDialogueSet.CharacterDialogues 的 backing field 注入 Scout 条目。
/// </summary>
public static class ScoutAncientDialogueHelper
{
	/// <summary>
	/// 在 DefineDialogues() 的 Postfix 中调用：往返回的 AncientDialogueSet 注入 Scout 对话。
	/// </summary>
	public static void InjectScoutDialogues(AncientDialogueSet dialogueSet, IReadOnlyList<AncientDialogue> scoutDialogues)
	{
		FieldInfo? field = typeof(AncientDialogueSet).GetField(
			"<CharacterDialogues>k__BackingField",
			BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			Log.Error("ScoutAncientDialogueHelper: Cannot find CharacterDialogues backing field!");
			return;
		}

		var dict = (Dictionary<string, IReadOnlyList<AncientDialogue>>)field.GetValue(dialogueSet);
		string scoutKey = ModelDb.Character<Scout>().Id.Entry;
		GD.Print($"[ScoutAncientDialogueHelper] Injecting Scout dialogues with key='{scoutKey}', dict already has {dict.Count} entries, contains SCOUT={dict.ContainsKey("SCOUT")}");

		if (dict.ContainsKey(scoutKey))
		{
			GD.Print($"[ScoutAncientDialogueHelper] Scout key '{scoutKey}' already exists, skipping.");
			return;
		}

		var newDict = new Dictionary<string, IReadOnlyList<AncientDialogue>>(dict)
		{
			// 与 AncientEventModel.CharKey<Scout>() 一致（返回角色 Id.Entry）
			[scoutKey] = scoutDialogues
		};
		field.SetValue(dialogueSet, newDict);
		GD.Print($"[ScoutAncientDialogueHelper] Successfully injected {scoutDialogues.Count} Scout dialogues with key='{scoutKey}'");
	}
}
