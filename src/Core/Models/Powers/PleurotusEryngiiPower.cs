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

	/// <summary>
	/// 回合结束时：若没有任何格挡，获得等同层数的格挡。
	/// </summary>
	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(Owner) && Owner.Block == 0)
		{
			Flash(); // 弹力菇图标闪烁，提示玩家触发了效果

			// 获得 6（8）点格挡
			await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
		}
	}
}
