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

namespace peak.Core.Models.Relics;

/// <summary>
/// 铆钉：第三回合开始时获得 3 点敏捷。
/// 罕见稀有度。
/// </summary>
public sealed class Rivet : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterSideTurnStart(
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner?.Creature))
		{
			return;
		}

		// 第 3 回合触发
		if (base.Owner?.PlayerCombatState?.TurnNumber == 3)
		{
			Flash();
			await PowerCmd.Apply<DexterityPower>(
				new ThrowingPlayerChoiceContext(),
				base.Owner.Creature,
				3m,
				base.Owner.Creature,
				null
			);
		}
	}
}
