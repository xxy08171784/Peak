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
/// 奶白金：获得 3 层人工制品。
/// 稀有稀有度，药水池，可在任意时刻使用，目标自身。
/// </summary>
public sealed class MilkPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;
	public override PotionUsage Usage => PotionUsage.AnyTime;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await PowerCmd.Apply<ArtifactPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
	}
}
