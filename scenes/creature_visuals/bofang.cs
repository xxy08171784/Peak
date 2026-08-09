using Godot;

public partial class bofang : AnimatedSprite2D
{
	[Export] public string IdleAnimation { get; set; } = "default";
	[Export] public string AttackAnimation { get; set; } = "attack";

	public override void _Ready()
	{
		GD.Print($"[bofang] _Ready! IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', SpriteFrames={(SpriteFrames != null ? "exists" : "NULL")}");
		AnimationFinished += OnAnimationFinished;
		Play(IdleAnimation);
		GD.Print($"[bofang] After Play(IdleAnimation), Animation='{Animation}', IsPlaying()={IsPlaying()}");
	}

	public void PlayAction(string action)
	{
		GD.Print($"[bofang] PlayAction called! action='{action}', Animation='{Animation}', IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', IsPlaying()={IsPlaying()}");
		if (action == "Attack")
		{
			// 已在播放攻击且正在播放中 → 不打断（去重）
			if (Animation == AttackAnimation && IsPlaying())
			{
				GD.Print("[bofang] Already playing attack, skipping");
				return;
			}

			// 动画已播完但 Animation 仍是 attack → 先切回待机再忽略本次触发
			if (Animation == AttackAnimation && !IsPlaying())
			{
				GD.Print("[bofang] Attack animation finished, switching to idle");
				Play(IdleAnimation);
				return;
			}

			// 正常播放攻击动画
			GD.Print($"[bofang] Playing attack animation! Play('{AttackAnimation}')");
			Play(AttackAnimation);
			GD.Print($"[bofang] After Play, Animation='{Animation}', IsPlaying()={IsPlaying()}");
		}
		else
		{
			GD.Print($"[bofang] Unknown action: '{action}', ignoring");
		}
	}

	private void OnAnimationFinished()
	{
		GD.Print($"[bofang] AnimationFinished! Animation='{Animation}', IsPlaying()={IsPlaying()}");
		if (Animation == AttackAnimation)
		{
			Play(IdleAnimation); // 攻击播完后自动回到待机
			GD.Print($"[bofang] Switched back to idle: '{IdleAnimation}'");
		}
	}
}
