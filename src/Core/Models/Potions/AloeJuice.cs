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
/// 芦荟汁：立即减少 35 炎热值。
/// 普通稀有度，角色专属，仅限战斗中使用，目标自身。
/// </summary>
public sealed class AloeJuice : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.Self;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		HeatPower? heat = base.Owner.Creature.GetPower<HeatPower>();
		if (heat != null && heat.Amount > 0)
		{
			int reduce = (int)System.Math.Min(heat.Amount, 35m);
			await PowerCmd.ModifyAmount(choiceContext, heat, -reduce, base.Owner.Creature, null);
		}
	}
}
