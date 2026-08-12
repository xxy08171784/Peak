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
/// 仙人掌汁：获得 6 点格挡和 2 点荆棘。
/// 普通稀有度，药水池，仅限战斗中使用，目标自身。
/// </summary>
public sealed class CactusJuice : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		Creature creature = base.Owner.Creature;
		await CreatureCmd.GainBlock(creature, 6m, ValueProp.Move, null);
		await PowerCmd.Apply<ThornsPower>(choiceContext, creature, 2m, creature, null);
	}
}
