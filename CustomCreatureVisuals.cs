using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace peak;

/// <summary>
/// Scout 角色战斗视觉根节点。
/// 继承游戏自带的 NCreatureVisuals，以便游戏将本场景识别为角色视觉场景。
/// 本项目使用 AnimatedSprite2D 帧动画（无 Spine 动画），bofang.cs 会自行管理播放。
/// </summary>
public partial class CustomCreatureVisuals : NCreatureVisuals
{
	/// <summary>场景中 %Visuals 节点（NCreatureVisuals 基类会在 _Ready 中设置为 _body）</summary>
	public Node2D? VisualsRoot { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		VisualsRoot = GetNodeOrNull<Node2D>("%Visuals");
	}

	/// <summary>
	/// 将游戏引擎的动画触发器（Attack/Hurt/Die 等）转发到帧动画。
	/// 由 Harmony 补丁 ScoutCreatureAnimPatch 调用。
	/// </summary>
	public void ForwardAnimationTrigger(string trigger)
	{
		GD.Print($"[CustomCreatureVisuals] ForwardAnimationTrigger called! trigger='{trigger}', VisualsRoot={(VisualsRoot != null ? "exists" : "NULL")}");
		if (VisualsRoot == null) return;
		var anim = VisualsRoot.GetNodeOrNull<bofang>("AnimatedSprite2D");
		GD.Print($"[CustomCreatureVisuals] bofang node: {(anim != null ? "found" : "NULL")}");
		anim?.PlayAction(trigger);
	}
}
