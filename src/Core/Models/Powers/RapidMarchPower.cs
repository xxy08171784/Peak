using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 急行军：每回合开始时，额外增加一次环境。
/// 配合【攀登】遗物的每回合自动 +1，本能力让每回合环境值共 +2。
/// </summary>
public sealed class RapidMarchPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（每层 = 每回合额外增加的环境次数）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 玩家回合开始时，额外增加环境。
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(Owner))
		{
			return;
		}

		MyClimbing? myClimbing = Owner.Player?.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing == null)
		{
			return;
		}

		// 每层急行军让环境额外 +1
		for (int i = 0; i < Amount; i++)
		{
			await myClimbing.ModifyEnvironmentValue(new ThrowingPlayerChoiceContext(), 1);
		}
	}
}
