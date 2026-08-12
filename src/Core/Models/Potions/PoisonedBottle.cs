using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Potions;

/// <summary>
/// 瓶装毒包：给予所有敌人 5 层中毒。
/// 普通稀有度，药水池，仅限战斗中使用，目标所有敌人。
/// </summary>
public sealed class PoisonedBottle : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AllEnemies;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		foreach (Creature enemy in base.Owner.Creature.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<ZhongduPower>(choiceContext, enemy, 5m, base.Owner.Creature, null);
		}
	}
}
