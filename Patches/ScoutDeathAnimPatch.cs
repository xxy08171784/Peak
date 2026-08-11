using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using peak;

namespace peak.Patches;

/// <summary>
/// Harmony 补丁：拦截游戏对 NCreature.StartDeathAnim 的调用。
///
/// 背景：游戏里死亡动画的触发位于 StartDeathAnim 内，且被
/// if (_spineAnimator != null) 包裹。Scout 使用 AnimatedSprite2D 帧动画，
/// 没有 Spine 视觉（SpineBody == null），因此 _spineAnimator 恒为 null，
/// 导致 SetAnimationTrigger("Dead") 永远不会被调用，死亡动画无法播放。
///
/// 此补丁在 StartDeathAnim 被调用时（无论 _spineAnimator 是否为 null），
/// 直接把 "Dead" 触发转发给 Scout 的帧动画（bofang.cs）。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class ScoutDeathAnimPatch
{
	static void Prefix(NCreature __instance)
	{
		// 只处理使用自定义帧动画视觉的角色（Scout）
		if (__instance.Visuals is CustomCreatureVisuals customVisuals)
		{
			Godot.GD.Print("[ScoutDeathAnimPatch] StartDeathAnim called! Forwarding 'Dead' to frame animation.");
			customVisuals.ForwardAnimationTrigger("Dead");
		}
	}
}
