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
/// 瓶装云雾：这回合受到的伤害减少一半（复用原版 DiamondDiadem 遗物的 BlurPower 机制）。
/// 罕见稀有度，药水池，仅限战斗中使用，目标自身。
/// </summary>
public sealed class BottledMist : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await PowerCmd.Apply<BlurPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
	}
}
