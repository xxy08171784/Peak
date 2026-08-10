using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

/// <summary>
/// 遮阳伞：在你的回合结束时，额外失去 9（12）点炎热。
/// 与散热（RejectionOfHeatPower）是独立能力，各自扣除炎热值。
/// </summary>
public sealed class SunshadePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 玩家回合结束时额外失去的炎热值层数。
	/// </summary>
	public int HeatLossPerTurn => Amount;

	/// <summary>
	/// 玩家回合结束时，额外失去等同于层数的炎热值。
	/// </summary>
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		// 只在宿主（玩家）自己的回合结束时触发
		if (side != Owner.Side)
		{
			return;
		}

		// 若当前没有炎热值则无事可做
		HeatPower? heatPower = Owner.GetPower<HeatPower>();
		if (heatPower == null || heatPower.Amount <= 0)
		{
			return;
		}

		Flash(); // 遮阳伞图标闪烁，提示玩家触发了效果

		// 额外扣除炎热值（不可为负）
		int toLose = Math.Min(heatPower.Amount, HeatLossPerTurn);
		await PowerCmd.ModifyAmount(choiceContext, heatPower, -toLose, Owner, null);
	}
}
