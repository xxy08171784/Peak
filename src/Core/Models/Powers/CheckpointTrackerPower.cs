using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 检查点追踪器 Power：在玩家的回合结束时，记录该玩家当前的生命值，
/// 供【检查点旗帜】回溯使用。
/// </summary>
public sealed class CheckpointTrackerPower : PowerModel
{
	// 正向增益（不可见的辅助标记）
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数固定为 1）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 玩家的回合结束时（CombatSide.Player），记录该玩家当前生命值。
	/// </summary>
	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player || Owner.Player == null || Owner.IsDead)
		{
			return;
		}

		// 记录玩家上回合结束时的生命值
		CheckpointFlagTracker.RecordTurnEndHp(Owner.Player, Owner.CurrentHp);
		await Task.CompletedTask;
	}
}
