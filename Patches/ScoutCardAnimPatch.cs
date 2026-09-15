using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using peak.Core.CardTypes;
using peak.Core.Models.Cards;

namespace peak.Patches;

/// <summary>
/// 按打出的卡牌决定给 Scout 发哪个动画触发器：
///   - 攻击牌：命中音效用 heavy_attack.mp3 → 大打（Attack），其余 → 小打（SmallAttack）；
///   - 食物牌（非攻击类）→ 食物动画（Food）；
///   - 防御牌 → 不播动画（原版对 DefendScout 本来也不发任何触发）。
///
/// 原版只会给攻击牌自动发 "Attack"（<c>AttackCommand.FromCard</c> 里
/// <c>_attackerAnimName = "Attack"</c>，<c>Execute</c> 里 TriggerAnim），食物/防御牌默认什么都不发，
/// 所以必须由这里补上。触发器最终由 ScoutCreatureAnimPatch → CustomCreatureVisuals → bofang.PlayAction 消费。
/// </summary>
[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
internal static class ScoutAttackSizePatch
{
	/// <summary><c>AttackCommand</c> 里存动画触发器名的私有字段。</summary>
	private static readonly FieldInfo? AttackerAnimNameField =
		AccessTools.Field(typeof(AttackCommand), "_attackerAnimName");

	[HarmonyPrefix]
	private static void UseHeavyOrSmallAttack(AttackCommand __instance)
	{
		// 只处理"玩家打出的攻击牌"：
		//   ModelSource is CardModel —— 排除 FromMonster（例如同样用这套帧动画的 Leader Miles）；
		//   GetCreatureNode().Visuals is CustomCreatureVisuals —— 只在本角色（帧动画）上生效。
		if (__instance.ModelSource is not CardModel)
		{
			return;
		}

		if (__instance.Attacker?.GetCreatureNode()?.Visuals is not CustomCreatureVisuals)
		{
			return;
		}

		if (AttackerAnimNameField == null)
		{
			GD.PrintErr("[ScoutCardAnimPatch] 找不到 AttackCommand._attackerAnimName 字段，大打/小打无法区分");
			return;
		}

		bool heavy = IsHeavyAttack(__instance.TmpHitSfx) || IsHeavyAttack(__instance.HitSfx);
		string animName = heavy ? "Attack" : "SmallAttack";
		AttackerAnimNameField.SetValue(__instance, animName);
		GD.Print($"[ScoutCardAnimPatch] {(heavy ? "大打" : "小打")} → trigger='{animName}'（hitSfx={__instance.TmpHitSfx ?? __instance.HitSfx ?? "null"}）");
	}

	/// <summary>大打判定：命中音效里带 heavy_attack（目前是 "heavy_attack.mp3"）。</summary>
	private static bool IsHeavyAttack(string? sfx)
		=> !string.IsNullOrEmpty(sfx)
			&& sfx.IndexOf("heavy_attack", StringComparison.OrdinalIgnoreCase) >= 0;
}

/// <summary>
/// 食物牌（非攻击类）打出时补一个 "Food" 触发器，让 bofang 播 scout_skill 的帧。
///
/// 同时是食物牌和攻击牌的 5 张卡（肉拳 FullBellyPunch / GastricPouch / Mushroom2 / SmallRoast /
/// 食神之怒 TheWrathOfTheFoodGod）走攻击动画，所以这里比对的是"声明行为类型"而不是 CardModel.Type——
/// PeakCardTypePatches 会让所有 IFoodCard 的 Type 一律返回 Food，拿不到 Attack。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class ScoutFoodAnimPatch
{
	[HarmonyPrefix]
	private static void PlayFoodAnimation(CardModel __instance)
	{
		if (__instance is not IFoodCard)
		{
			return;
		}

		if (PeakCardTypes.GetDeclaredBehaviorType(__instance) == CardType.Attack)
		{
			return;
		}

		Creature? creature = __instance.Owner?.Creature;
		NCreature? creatureNode = creature?.GetCreatureNode();
		if (creatureNode?.Visuals is CustomCreatureVisuals)
		{
			creatureNode.SetAnimationTrigger("Food");
		}
	}
}
