using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using peak;

namespace peak.Patches;

/// <summary>
/// Harmony 补丁：拦截游戏对 NCreature.SetAnimationTrigger 的调用，
/// 将攻击/受击/死亡等动画触发器转发到 Scout 的帧动画（bofang.cs）。
/// 游戏自动驱动 Spine 骨骼动画，但 Scout 使用 AnimatedSprite2D 帧动画，
/// 需要此补丁桥接。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class ScoutCreatureAnimPatch
{
	static void Postfix(NCreature __instance, string trigger)
	{
		Godot.GD.Print($"[ScoutAnimPatch] SetAnimationTrigger called! trigger='{trigger}', Visuals type={__instance.Visuals?.GetType()?.FullName}");
		if (__instance.Visuals is CustomCreatureVisuals customVisuals)
		{
			Godot.GD.Print("[ScoutAnimPatch] Visuals is CustomCreatureVisuals, forwarding...");
			customVisuals.ForwardAnimationTrigger(trigger);
		}
		else
		{
			Godot.GD.Print($"[ScoutAnimPatch] Visuals is NOT CustomCreatureVisuals, it's: {__instance.Visuals?.GetType()?.FullName ?? "null"}");
		}
	}
}