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
/// 药水形状的雪球：给予所有敌人 2 层寒冷。
/// 罕见稀有度，角色专属，仅限战斗中使用，目标所有敌人。
/// </summary>
public sealed class SnowballPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AllEnemies;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		foreach (Creature enemy in base.Owner.Creature.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<ColdPower>(choiceContext, enemy, 2m, base.Owner.Creature, null);
		}
	}
}
