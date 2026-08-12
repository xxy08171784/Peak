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
using peak.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 蜱虫：每回合给与随机敌人 2 层中毒。
/// 普通稀有度。
/// </summary>
public sealed class Tick : RelicModel
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

		List<Creature> enemies = combatState.HittableEnemies.ToList();
		if (enemies.Count == 0)
		{
			return;
		}

		// 随机选一名敌人
		Creature? target = base.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
		if (target == null)
		{
			return;
		}

		Flash();
		await PowerCmd.Apply<ZhongduPower>(
			new ThrowingPlayerChoiceContext(),
			target,
			2m,
			base.Owner.Creature,
			null
		);
	}
}
