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
/// 渐冻：在你的回合结束时，所有拥有渐冻的敌人受到 10 × 渐冻层数 点伤害。
/// 伤害值与渐冻层数绑定，与自身层数无关。
/// </summary>
public sealed class GraduallyFreezingPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数固定为 1，仅作标记）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	// 每层渐冻造成的伤害
	private const decimal DamagePerFrostbiteStack = 10m;

	/// <summary>
	/// 玩家回合结束时，对所有拥有渐冻的敌人造成 10 × 渐冻层数 点伤害。
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

		// 对每个拥有渐冻的敌人造成 10 × 渐冻层数 点伤害（失去生命 = 不可被格挡）
		foreach (var enemy in frostbittenEnemies)
		{
			int frostbiteStacks = enemy.GetPower<FrostbitePower>()?.Amount ?? 0;
			if (frostbiteStacks <= 0)
			{
				continue;
			}

			await CreatureCmd.Damage(
				choiceContext,
				enemy,
				DamagePerFrostbiteStack * frostbiteStacks,
				ValueProp.Unpowered,
				Owner
			);
		}
	}
}
