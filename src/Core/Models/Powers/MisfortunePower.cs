using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 厄运：每当你给予敌人一次负面状态时，对随机敌人造成伤害（每层 7/9 点）。
/// Amount 即每次触发的伤害值，多张厄运可叠加。
/// </summary>
public sealed class MisfortunePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 每次触发伤害值）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每次给予敌人负面状态时造成的伤害值。
	/// </summary>
	public int DamagePerTrigger => Amount;

	/// <summary>
	/// 监听全局 power 变化：当玩家对敌人施加 debuff 时，对随机敌人造成伤害。
	/// </summary>
	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		// 只触发：层数增加 + 是 debuff + 在敌人身上 + 是玩家施加 + 非临时性
		if (amount > 0
			&& power.GetTypeForAmount(amount) == PowerType.Debuff
			&& power.Owner.IsEnemy
			&& applier == base.Owner
			&& power is not ITemporaryPower)
		{
			Flash(); // 厄运图标闪烁

			// 对随机敌人造成伤害
			List<Creature> aliveEnemies = base.Owner.CombatState.HittableEnemies.ToList();
			if (aliveEnemies.Count == 0)
			{
				return;
			}

			Creature? target = base.Owner.Player?.RunState.Rng.CombatTargets.NextItem(aliveEnemies);
			if (target == null)
			{
				return;
			}

			await CreatureCmd.Damage(
				choiceContext,
				target,
				(decimal)DamagePerTrigger,
				ValueProp.Unpowered,
				base.Owner
			);
		}
	}
}
