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
}
