using Godot;

public partial class ScoutAnimatedSprite : AnimatedSprite2D
{
	public override void _Ready()
	{
		// 进入场景后自动播放 default 待机动画
		Play("default");
	}
}
