using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 缠绕：你每次打出一张牌时，这个敌人失去 1（2）点生命。
/// 这是施加在敌人身上的 debuff（负面状态）。
/// </summary>
public sealed class TwinePower : PowerModel
{
	// 负面状态
	public override PowerType Type => PowerType.Debuff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每次打出一张牌时敌人失去的生命值。
	/// </summary>
	public int DamagePerCard => Amount;

	/// <summary>
	/// 每当玩家打出一张牌时，对这个敌人造成 1（2）点伤害。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只对打出牌的玩家生效（缠绕的敌人是 Owner）
		if (cardPlay.Card.Owner.Creature.Side == Owner.Side)
		{
			return;
		}

		Flash(); // 缠绕图标闪烁，提示触发了效果

		// 敌人失去等同于层数的生命
		await CreatureCmd.Damage(
			choiceContext,
			Owner,
			DamagePerCard,
			ValueProp.Unblockable | ValueProp.Unpowered,
			null,
			null
		);
	}
}
