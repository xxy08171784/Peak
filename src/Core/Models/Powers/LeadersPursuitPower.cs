using System.Collections.Generic;
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
/// 领队追杀：在你的回合结束时，受到 5 点伤害。
/// </summary>
public sealed class LeadersPursuitPower : PowerModel
{
	// 负面效果，属于 Debuff
	public override PowerType Type => PowerType.Debuff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 玩家回合结束时受到的自伤值（每层）。
	/// </summary>
	public int DamagePerTurn => Amount * 5;

	/// <summary>
	/// 玩家回合结束时，受到 5 点伤害（可被格挡）。
	/// </summary>
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		// 只在宿主（玩家）自己的回合结束时触发
		if (side != Owner.Side)
		{
			return;
		}

		Flash(); // 领队追杀图标闪烁，提示玩家触发了效果

		// 对自己造成伤害（每层 5 点，可被格挡）
		await CreatureCmd.Damage(
			choiceContext,
			Owner,
			(decimal)DamagePerTurn,
			ValueProp.Unpowered | ValueProp.Move,
			Owner
		);
	}
}
