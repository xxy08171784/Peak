using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 玩家版中毒（zhongdu）。
/// 效果与原版 PoisonPower 相同，但改为在【回合结束时】对玩家造成伤害。
/// 怪物身上的中毒不受影响，仍使用原版 PoisonPower。
/// 图标使用 zhongdu_power.png（由 Godot 按类名自动加载）。
/// </summary>
public sealed class ZhongduPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public override bool AllowNegative => false;

	/// <summary>计算触发次数（与原版逻辑一致）</summary>
	private int TriggerCount
	{
		get
		{
			int extra = Owner.CombatState?.GetOpponentsOf(Owner)?.Sum(c => c?.GetPowerAmount<AccelerantPower>() ?? 0) ?? 0;
			return Math.Min((int)Amount, 1 + extra);
		}
	}

	/// <summary>回合开始时不做任何事（跳过原版中毒的回合开始触发）</summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		await Task.CompletedTask;
	}

	/// <summary>回合结束时：对玩家造成中毒伤害</summary>
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != Owner.Side)
			return;

		int count = TriggerCount;
		for (int i = 0; i < count; i++)
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
			if (Owner.IsAlive)
				await PowerCmd.Decrement(this);
			else
				await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
	}
}