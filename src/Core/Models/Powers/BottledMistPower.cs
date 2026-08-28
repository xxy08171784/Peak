using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 瓶装云雾：本回合受到的伤害减少一半。
/// </summary>
public sealed class BottledMistPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 不堆叠，单层效果
	public override PowerStackType StackType => PowerStackType.Single;

	/// <summary>
	/// 将受到的伤害减半（向下取整）。
	/// </summary>
	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return amount;
		}

		// 伤害减半
		return amount / 2m;
	}

	/// <summary>
	/// 效果触发时闪烁图标。
	/// </summary>
	public override async Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		await Task.CompletedTask;
	}

	/// <summary>
	/// 玩家回合结束时移除本效果（仅持续一回合）。
	/// </summary>
	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Player && participants.Contains(base.Owner))
		{
			await PowerCmd.Remove(this);
		}
	}
}