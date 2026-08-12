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

namespace peak.Core.Models.Powers;

/// <summary>
/// 魔豆汁：每回合开始时获得 1 点力量和 1 点敏捷，持续 3 回合。
/// Amount 即剩余回合数（施加时为 3），每回合递减，归零自动移除。
/// </summary>
public sealed class MagicBeanPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public override bool AllowNegative => false;

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner) || Owner.IsDead)
		{
			return;
		}

		Flash();

		// 获得 1 力量 + 1 敏捷
		await PowerCmd.Apply<StrengthPower>(
			new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
		await PowerCmd.Apply<DexterityPower>(
			new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);

		// 递减剩余回合数，归零移除
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
