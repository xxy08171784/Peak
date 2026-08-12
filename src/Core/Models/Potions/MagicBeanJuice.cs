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
/// 魔豆汁：回合开始时获得 1 力量 1 敏捷，持续 3 回合。
/// 罕见稀有度，药水池，仅限战斗中使用，目标自身。
/// </summary>
public sealed class MagicBeanJuice : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		await PowerCmd.Apply<MagicBeanPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
	}
}
