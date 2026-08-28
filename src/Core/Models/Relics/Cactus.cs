using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 仙人掌：本场战斗第一次受到攻击时，对攻击者造成 10 点伤害。
/// 普通稀有度。
/// </summary>
public sealed class Cactus : RelicModel
{
	private bool _triggeredThisCombat;

	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			_triggeredThisCombat = false;
			base.Status = RelicStatus.Active;
		}
	}

	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult result,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		if (_triggeredThisCombat || dealer == null || target != base.Owner?.Creature)
		{
			return;
		}

		// 自己打自己的伤害（放血、烙印等）不触发仙人掌反击
		if (dealer == target)
		{
			return;
		}

		_triggeredThisCombat = true;
		base.Status = RelicStatus.Normal;
		Flash();

		await CreatureCmd.Damage(
			choiceContext,
			dealer,
			10m,
			ValueProp.Unpowered,
			base.Owner!.Creature
		);
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		_triggeredThisCombat = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
