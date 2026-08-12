using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 风滚草：在第二回合时，对所有敌人造成 14 点伤害。
/// 罕见稀有度。
/// </summary>
public sealed class Tumbleweed : RelicModel
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

		// 仅第 2 回合触发
		if (base.Owner?.PlayerCombatState?.TurnNumber != 2)
		{
			return;
		}

		List<Creature> enemies = combatState.HittableEnemies.ToList();
		if (enemies.Count == 0)
		{
			return;
		}

		Flash();
		await CreatureCmd.Damage(
			new ThrowingPlayerChoiceContext(),
			enemies,
			14m,
			ValueProp.Unpowered,
			base.Owner!.Creature
		);
	}
}
