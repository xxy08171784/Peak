using Godot;

public partial class bofang : AnimatedSprite2D
{
	[Export] public string IdleAnimation { get; set; } = "default";
	[Export] public string AttackAnimation { get; set; } = "attack";
	[Export] public string DeathAnimation { get; set; } = "death";

	private bool _isDead = false;

	public override void _Ready()
	{
		GD.Print($"[bofang] _Ready! IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', DeathAnimation='{DeathAnimation}', SpriteFrames={(SpriteFrames != null ? "exists" : "NULL")}");
		AnimationFinished += OnAnimationFinished;
		Play(IdleAnimation);
		GD.Print($"[bofang] After Play(IdleAnimation), Animation='{Animation}', IsPlaying()={IsPlaying()}");
	}

	public void PlayAction(string action)
	{
		GD.Print($"[bofang] PlayAction called! action='{action}', Animation='{Animation}', IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', DeathAnimation='{DeathAnimation}', IsPlaying()={IsPlaying()}");

		// 死亡后不再响应其他动作（保持倒地姿态）
		if (_isDead)
		{
			GD.Print("[bofang] Already dead, ignoring action");
			return;
		}

		if (action == "Dead")
		{
			// 播放死亡动画（不循环），播完后由 AnimationFinished 处理停在最后一帧
			_isDead = true;
			GD.Print($"[bofang] Playing death animation! Play('{DeathAnimation}')");
			Play(DeathAnimation);
			return;
		}

		if (action == "Revive")
		{
			// 复活：解除死亡状态并回到待机
			_isDead = false;
			GD.Print("[bofang] Reviving, switching to idle");
			Play(IdleAnimation);
			return;
		}

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
		else if (_isDead)
		{
			// 死亡动画（非循环）播完后 Godot 会自然停在最后一帧（IsPlaying()=False）。
			// 注意：不要调用 Stop()——Godot 4.5 的 Stop() 会把 Frame 重置为 0，
			// 导致角色从倒地姿态弹回站立。这里只做无副作用的钉帧兜底（Frame 已是最后一帧时是 no-op）。
			int lastFrame = SpriteFrames.GetFrameCount(DeathAnimation) - 1;
			if (lastFrame >= 0)
			{
				Frame = lastFrame;
				QueueRedraw();
			}
			GD.Print($"[bofang] Death anim finished, pinned Frame={lastFrame}. Animation='{Animation}', Frame={Frame}, IsPlaying()={IsPlaying()}");
		}
	}
}
