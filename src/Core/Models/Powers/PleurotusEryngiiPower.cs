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
/// 弹力菇：如果你在回合结束时没有任何格挡，获得 6（8）点格挡。
/// </summary>
public sealed class PleurotusEryngiiPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	private bool _shouldTrigger;

	/// <summary>
	/// 每回合开始时重置标记。
	/// </summary>
	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		_shouldTrigger = false;
		return Task.CompletedTask;
	}

	/// <summary>
	/// 极早期阶段检查格挡状态（早于 PlatingPower 触发）。
	/// 此时覆甲还没给盾，能正确判断 Owner.Block == 0。
	/// </summary>
	public override Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner))
		{
			return Task.CompletedTask;
		}
		_shouldTrigger = Owner.Block == 0;
		return Task.CompletedTask;
	}

	/// <summary>
	/// 回合结束时（与覆甲同时触发）：根据 VeryEarly 阶段的判断，给予格挡。
	/// </summary>
	public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!_shouldTrigger)
		{
			return;
		}
		_shouldTrigger = false;
		Flash(); // 弹力菇图标闪烁，提示玩家触发了效果

		// 获得 6（8）点格挡
		await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
	}
}
