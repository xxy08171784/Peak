using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace peak;

/// <summary>
/// Scout 商店角色视觉脚本。
/// 继承游戏自带的 NMerchantCharacter，以便 NMerchantRoom 通过
/// Character.MerchantAnimPath 加载场景时能正确实例化（Instantiate&lt;NMerchantCharacter&gt;）。
/// 本项目使用 AnimatedSprite2D 帧动画（无 Spine），因此覆写 _Ready 直接播放待机帧动画，
/// 跳过基类对 SpineSprite 的等待逻辑。
/// </summary>
public partial class ScoutMerchant : NMerchantCharacter
{
	private AnimatedSprite2D? _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_sprite != null)
		{
			_sprite.Play("default");
		}
	}
}
