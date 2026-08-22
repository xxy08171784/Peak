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
/// 暖宝宝：在你的回合开始时，获得 6（9）点炎热。
/// 炎热值获得会由 HeatPower.AfterPowerAmountChanged 自动补充散热。
/// 注意：卡牌 OnPlay 中的立即获得 9/13 炎热在 Power 外部处理。
/// </summary>
public sealed class WarmPastePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（每层叠加每回合获得的炎热值）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每回合开始时获得的炎热值层数。
	/// </summary>
	public int HeatPerTurn => Amount;

	/// <summary>
	/// 玩家回合开始时，获得等同于层数的炎热值。
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		// 只在玩家自己的回合开始时触发
		if (!participants.Contains(Owner))
		{
			return;
		}

		Flash(); // 暖宝宝图标闪烁，提示玩家触发了效果

		// 获得炎热值（PowerCmd.Apply 会叠加层数并触发 HeatPower 的散热补充）
		await PowerCmd.Apply<HeatPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
	}
}
