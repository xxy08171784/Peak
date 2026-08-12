using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 绊脚石：战斗开始时给与随机一名敌人 1 层易伤和 1 层虚弱。
/// 普通稀有度。
/// </summary>
public sealed class StumblingBlock : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		List<Creature> enemies = base.Owner.Creature.CombatState.HittableEnemies.ToList();
		if (enemies.Count == 0)
		{
			return;
		}

		Creature? target = base.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
		if (target == null)
		{
			return;
		}

		Flash();
		var ctx = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<VulnerablePower>(ctx, target, 1m, base.Owner.Creature, null);
		await PowerCmd.Apply<WeakPower>(ctx, target, 1m, base.Owner.Creature, null);
	}
}
