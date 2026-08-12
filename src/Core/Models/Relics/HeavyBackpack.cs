using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 沉甸甸的行囊：打出两费及以上的牌时对随机敌人造成 8 点伤害。
/// 罕见稀有度。
/// </summary>
public sealed class HeavyBackpack : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner || base.Owner?.Creature == null)
		{
			return;
		}

		// 检查原始费用 >= 2
		if (cardPlay.Card.EnergyCost.Canonical < 2)
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
		await CreatureCmd.Damage(
			choiceContext,
			target,
			8m,
			ValueProp.Unpowered,
			base.Owner.Creature
		);
	}
}
