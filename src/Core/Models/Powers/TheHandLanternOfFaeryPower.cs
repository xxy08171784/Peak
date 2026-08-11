using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

/// <summary>
/// 仙子提灯：4（5）回合内，当你的回合结束时，回复 2 生命，降低 10 炎热，降低 5 中毒。
/// Amount 即剩余回合数（施加时为 4/5），每回合递减，归零自动移除。
/// </summary>
public sealed class TheHandLanternOfFaeryPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 剩余回合数）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 回合结束时（在所有其他 AfterSideTurnEnd 之后）：回复 2 生命，降低 10 炎热，降低 5 中毒，然后递减剩余回合数。
	/// 使用 Late 确保在散热等 power 之后执行，不干扰其他效果。
	/// </summary>
	public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.IsDead)
		{
			return;
		}

		Flash(); // 仙子提灯图标闪烁，提示玩家触发了效果

		// 1. 回复 2 生命
		await CreatureCmd.Heal(Owner, 2m);

		// 2. 降低 10 炎热
		HeatPower? heat = Owner.GetPower<HeatPower>();
		if (heat != null && heat.Amount > 0)
		{
			int reduceHeat = (int)System.Math.Min(heat.Amount, 10m);
			await PowerCmd.ModifyAmount(choiceContext, heat, -reduceHeat, Owner, null);
		}

		// 3. 降低 5 中毒
		ZhongduPower? zhongdu = Owner.GetPower<ZhongduPower>();
		if (zhongdu != null && zhongdu.Amount > 0)
		{
			int reduceZhongdu = (int)System.Math.Min(zhongdu.Amount, 5m);
			await PowerCmd.ModifyAmount(choiceContext, zhongdu, -reduceZhongdu, Owner, null);
		}

		// 4. 递减剩余回合数（Amount - 1），归零时自动移除
		if (Amount > 1)
		{
			await PowerCmd.Decrement(this);
		}
		else
		{
			await PowerCmd.Remove(this);
		}
	}
}
