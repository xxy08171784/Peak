using Godot;

/// <summary>
/// 极简待机动画播放器。
/// 用于商人和休息处场景——不需要响应任何战斗触发器，只播放默认待机动画。
/// </summary>
public partial class SimpleIdle : AnimatedSprite2D
{
	public override void _Ready()
	{
		if (SpriteFrames == null)
		{
			CallDeferred(nameof(Retry));
			return;
		}
		Play("default");
	}

	private void Retry()
	{
		if (SpriteFrames == null) return;
		Play("default");
	}
}