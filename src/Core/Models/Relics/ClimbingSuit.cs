using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山服：每回合开始时获得 2 层覆甲。
/// 稀有稀有度。
/// </summary>
public sealed class ClimbingSuit : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterSideTurnStart(
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner?.Creature))
		{
			return;
		}

		Flash();
		var ctx = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<PlatingPower>(ctx, base.Owner.Creature, 2m, base.Owner.Creature, null);
	}
}
