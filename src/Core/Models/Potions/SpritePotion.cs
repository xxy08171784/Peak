using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Potions;

/// <summary>
/// 雪碧：获得 3 点能量。
/// 罕见稀有度，药水池，仅限战斗中使用，目标自身。
/// </summary>
public sealed class SpritePotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await PlayerCmd.GainEnergy(3m, base.Owner);
	}
}
