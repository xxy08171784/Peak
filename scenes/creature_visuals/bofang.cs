using Godot;

public partial class bofang : AnimatedSprite2D
{
	[Export] public string IdleAnimation { get; set; } = "default";
	[Export] public string AttackAnimation { get; set; } = "attack";           // 大打：命中音效为 heavy_attack 的攻击牌
	[Export] public string SmallAttackAnimation { get; set; } = "attack_small"; // 小打：其余攻击牌
	[Export] public string DeathAnimation { get; set; } = "death";
	[Export] public string HitAnimation { get; set; } = "hit";                 // 受击（用的是 scout_defend 的帧）
	[Export] public string SkillAnimation { get; set; } = "skill";             // 食物牌

	private bool _isDead = false;

	public override void _Ready()
	{
		GD.Print($"[bofang] _Ready! IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', SmallAttackAnimation='{SmallAttackAnimation}', DeathAnimation='{DeathAnimation}', HitAnimation='{HitAnimation}', SkillAnimation='{SkillAnimation}', SpriteFrames={(SpriteFrames != null ? "exists" : "NULL")}");
		AnimationFinished += OnAnimationFinished;
		Play(IdleAnimation);
		GD.Print($"[bofang] After Play(IdleAnimation), Animation='{Animation}', IsPlaying()={IsPlaying()}");
	}

	public void PlayAction(string action)
	{
		GD.Print($"[bofang] PlayAction called! action='{action}', Animation='{Animation}', IdleAnimation='{IdleAnimation}', AttackAnimation='{AttackAnimation}', DeathAnimation='{DeathAnimation}', IsPlaying()={IsPlaying()}");

		// 游戏发来的触发既有 PascalCase（Attack/Dead），也有 Spine 蛇形动画名（hurt/block/cast/...），
		// 统一转小写后匹配，两种写法都能识别。
		string key = (action ?? string.Empty).Trim().ToLowerInvariant();

		// 复活：解除死亡状态并回到待机（放在 _isDead 拦截之前，否则复活永远无法生效）
		if (key == "revive" || key == "reset")
		{
			_isDead = false;
			GD.Print("[bofang] Reviving, switching to idle");
			Play(IdleAnimation);
			return;
		}

		// 死亡后不再响应其他动作（保持倒地姿态）
		if (_isDead)
		{
			GD.Print("[bofang] Already dead, ignoring action");
			return;
		}

		// 死亡
		if (key == "dead" || key == "die" || key == "death" || key.StartsWith("die"))
		{
			// 播放死亡动画（不循环），播完后由 AnimationFinished 处理停在最后一帧
			_isDead = true;
			GD.Print($"[bofang] Playing death animation! Play('{DeathAnimation}')");
			Play(DeathAnimation);
			return;
		}

		// 小打：命中音效不含 heavy_attack 的攻击牌（补丁把触发器名改成了 SmallAttack）
		if (key == "smallattack" || key == "attack_small")
		{
			PlayAttack(SmallAttackAnimation, "small attack");
			return;
		}

		// 大打（含 attack_1 / attack_slash 等蛇形变体）
		if (key == "attack" || key.StartsWith("attack"))
		{
			PlayAttack(AttackAnimation, "big attack");
			return;
		}

		// 受击（hurt / hit / hurt_* / stunned_hurt 等）—— 用 scout_defend 的帧
		if (key == "hurt" || key == "hit" || key == "damaged" || key == "takedamage" || key.StartsWith("hurt"))
		{
			PlayOneShot(HitAnimation, "hit");
			return;
		}

		// 食物牌（food / skill / cast 等）—— 用 scout_skill 的帧；防御牌不播动画
		if (key == "food" || key == "skill" || key == "cast" || key == "special" || key.StartsWith("cast"))
		{
			PlayOneShot(SkillAnimation, "food");
			return;
		}

		// 待机（idle / idle_loop / glow_idle 等）
		if (key == "idle" || key == "glow_idle" || key.StartsWith("idle"))
		{
			Play(IdleAnimation);
			return;
		}

		GD.Print($"[bofang] Unknown action: '{action}', ignoring");
	}

	/// <summary>
	/// 播放攻击动画（大打 / 小打），播完后由 AnimationFinished 回到待机。
	/// 游戏对同一次攻击可能连发多次触发，这里做去重；若上一次已经播完，先切回待机。
	/// </summary>
	private void PlayAttack(string animationName, string label)
	{
		if (SpriteFrames == null || !SpriteFrames.HasAnimation(animationName))
		{
			GD.Print($"[bofang] Animation '{animationName}' not found in SpriteFrames, ignoring {label}");
			return;
		}

		// 同一动画已在播放 → 不打断（去重）
		if (Animation == animationName && IsPlaying())
		{
			GD.Print($"[bofang] Already playing {label}, skipping");
			return;
		}

		// 动画已播完但 Animation 仍是同一个 → 先切回待机再忽略本次触发
		if (Animation == animationName && !IsPlaying())
		{
			GD.Print($"[bofang] {label} animation finished, switching to idle");
			Play(IdleAnimation);
			return;
		}

		GD.Print($"[bofang] Playing {label} animation! Play('{animationName}')");
		Play(animationName);
		GD.Print($"[bofang] After Play, Animation='{Animation}', IsPlaying()={IsPlaying()}");
	}

	/// <summary>
	/// 播放一次性的受击 / 食物动画，播完后由 AnimationFinished 回到待机。
	/// </summary>
	private void PlayOneShot(string animationName, string label)
	{
		if (SpriteFrames == null || !SpriteFrames.HasAnimation(animationName))
		{
			GD.Print($"[bofang] Animation '{animationName}' not found in SpriteFrames, ignoring {label}");
			return;
		}

		// 同一动画已在播放 → 不打断（去重）
		if (Animation == animationName && IsPlaying())
		{
			GD.Print($"[bofang] Already playing {label}, skipping");
			return;
		}

		GD.Print($"[bofang] Playing {label} animation! Play('{animationName}')");
		Play(animationName);
	}

	private void OnAnimationFinished()
	{
		GD.Print($"[bofang] AnimationFinished! Animation='{Animation}', IsPlaying()={IsPlaying()}");

		if (_isDead)
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
			return;
		}

		// 攻击（大打/小打）/ 受击 / 食物 播完后自动回到待机；防御牌不播动画，故没有 defend 分支
		if (Animation == AttackAnimation || Animation == SmallAttackAnimation
			|| Animation == HitAnimation || Animation == SkillAnimation)
		{
			Play(IdleAnimation);
			GD.Print($"[bofang] Switched back to idle: '{IdleAnimation}'");
		}
	}
}
