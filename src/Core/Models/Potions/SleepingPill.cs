using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Potions;

/// <summary>
/// 安眠药：给所有敌人 2 层虚弱。
/// 普通稀有度，药水池，仅限战斗中使用，目标所有敌人。
/// </summary>
public sealed class SleepingPill : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AllEnemies;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		foreach (Creature enemy in base.Owner.Creature.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 2m, base.Owner.Creature, null);
		}
	}
}
