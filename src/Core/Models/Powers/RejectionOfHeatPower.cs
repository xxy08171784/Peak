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
/// 散热：玩家回合结束时，失去 7 点炎热值。
/// 炎热值自身不会下降，全靠此能力在回合结束时扣除。
/// 玩家首次获得炎热值时自动获得 1 层散热。
/// </summary>
public sealed class RejectionOfHeatPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（固定 1 层，逻辑上不叠加扣减）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每回合结束时扣除的炎热值层数。
	/// </summary>
	private const int HeatLossPerTurn = 7;

	/// <summary>
	/// 玩家回合结束时，失去 7 点炎热值。
	/// 炎热值减少会由 HeatPower.AfterPowerAmountChanged 触发随机敌人伤害。
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

		Flash(); // 散热图标闪烁，提示玩家触发了效果

		// 扣除 7 点炎热值（不可为负）
		int toLose = Math.Min(heatPower.Amount, HeatLossPerTurn);
		await PowerCmd.ModifyAmount(choiceContext, heatPower, -toLose, Owner, null);
	}
}
