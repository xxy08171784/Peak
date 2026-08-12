using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 望远镜：前两回合额外抽 1 张牌。
/// 普通稀有度。
/// </summary>
public sealed class Telescope : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task AfterSideTurnStart(
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner?.Creature))
		{
			return;
		}

		// 仅前两回合（TurnNumber 1 和 2）
		if (base.Owner?.PlayerCombatState?.TurnNumber <= 2)
		{
			Flash();
			await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1m, base.Owner);
		}
	}
}
