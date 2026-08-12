using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Potions;

/// <summary>
/// 急冻瓶：眩晕一名敌人，使其跳过本回合行动。
/// 稀有稀有度，药水池，仅限战斗中使用，目标单个敌人。
/// </summary>
public sealed class FreezeBottle : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AnyEnemy;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		AssertValidForTargetedPotion(target);
		await CreatureCmd.Stun(target);
	}
}
