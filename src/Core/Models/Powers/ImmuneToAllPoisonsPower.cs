using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 百毒不侵：中毒不会对自己造成伤害，并且每当你失去 1 层中毒，就给予所有敌人 2 层中毒（原版中毒 PoisonPower）。
/// </summary>
public sealed class ImmuneToAllPoisonsPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每失去 1 层中毒给予敌人的中毒层数。
	/// </summary>
	public int PoisonPerLoss => Amount;

	/// <summary>
	/// 每当中毒层数变化时触发：失去中毒时，给所有敌人施加 2 倍层数的中毒（原版中毒）。
	/// </summary>
	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		// 只处理玩家身上的 ZhongduPower 层数变化
		if (power is not ZhongduPower || power.Owner != Owner)
		{
			return;
		}

		// 只有失去中毒（amount < 0）才触发
		if (amount >= 0)
		{
			return;
		}

		int layersLost = (int)-amount; // amount 为负，取绝对值即失去的层数
		if (layersLost <= 0)
		{
			return;
		}

		Flash(); // 百毒不侵图标闪烁，提示玩家触发了效果

		// 给所有敌人施加 2 倍失去层数的原版中毒（PoisonPower）
		IEnumerable<Creature> enemies = Owner.CombatState.HittableEnemies.Where(c => c.IsAlive);
		foreach (Creature enemy in enemies)
		{
			await PowerCmd.Apply<PoisonPower>(
				choiceContext,
				enemy,
				layersLost * 2,
				Owner,
				null
			);
		}
	}

	/// <summary>
	/// 给所有敌人施加指定层数的原版中毒（供 ZhongduPower.AfterRemoved 调用）。
	/// </summary>
	public async Task ApplyPoisonToAllEnemies(PlayerChoiceContext choiceContext, int layers)
	{
		if (layers <= 0)
		{
			return;
		}

		Flash(); // 百毒不侵图标闪烁，提示玩家触发了效果

		IEnumerable<Creature> enemies = Owner.CombatState.HittableEnemies.Where(c => c.IsAlive);
		foreach (Creature enemy in enemies)
		{
			await PowerCmd.Apply<PoisonPower>(
				choiceContext,
				enemy,
				layers,
				Owner,
				null
			);
		}
	}
}
