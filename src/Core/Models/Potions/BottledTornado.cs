using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Potions;

/// <summary>
/// 瓶装龙卷风：对所有敌人造成 25 点伤害，给予 2 层易伤。
/// 稀有稀有度，药水池，仅限战斗中使用，目标所有敌人。
/// </summary>
public sealed class BottledTornado : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AllEnemies;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		Creature owner = base.Owner.Creature;
		foreach (Creature enemy in owner.CombatState.HittableEnemies)
		{
			await CreatureCmd.Damage(choiceContext, enemy, 25m, ValueProp.Unpowered, owner);
			await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 2m, owner, null);
		}
	}
}
