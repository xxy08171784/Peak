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
/// 疲劳：在下回合开始时，失去1点能量。
/// </summary>
public sealed class TiredPower : PowerModel
{
	// 负面效果，属于 Debuff
	public override PowerType Type => PowerType.Debuff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 玩家回合开始时，失去等同于层数的能量。
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		// 只在宿主（玩家）自己的回合开始时触发
		if (!participants.Contains(Owner))
		{
			return;
		}

		Flash(); // 疲劳图标闪烁，提示玩家触发了效果

		// 失去能量（每层 1 点）
		await PlayerCmd.LoseEnergy(Amount, Owner.Player);
	}
}