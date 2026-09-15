using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 渐冻：在你的回合结束时，对所有拥有渐冻的敌人造成伤害（12/16 点）。
/// Amount 即每回合造成的伤害值，多张渐冻可叠加层数。
/// </summary>
public sealed class GraduallyFreezingPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 每回合伤害值）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每回合对拥有渐冻的敌人造成的伤害值。
	/// </summary>
	public int DamagePerTurn => Amount;

	/// <summary>
	/// 玩家回合结束时，对所有拥有渐冻的敌人造成 Amount 点伤害。
	/// </summary>
	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		// 只在玩家回合结束时触发
		if (side != CombatSide.Player || !participants.Contains(Owner))
		{
			return;
		}

		// 获取所有拥有渐冻的敌人
		List<Creature> frostbittenEnemies = Owner.CombatState.HittableEnemies
			.Where(e => e.GetPower<FrostbitePower>() != null)
			.ToList();

		if (frostbittenEnemies.Count == 0)
		{
			return;
		}

		Flash(); // 渐冻图标闪烁，提示玩家触发了效果

		// 对每个拥有渐冻的敌人造成 Amount 点伤害（失去生命 = 不可被格挡）
		foreach (var enemy in frostbittenEnemies)
		{
			await CreatureCmd.Damage(
				choiceContext,
				enemy,
				(decimal)DamagePerTurn,
				ValueProp.Unpowered,
				Owner
			);
		}
	}
}
