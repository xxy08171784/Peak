using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 冷库：拥有寒冷或渐冻状态的敌人造成的伤害下降 25%。
/// 稀有稀有度。
/// </summary>
public sealed class ColdStorage : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override decimal ModifyHpLostAfterOsty(
		Creature target,
		decimal amount,
		ValueProp props,
		Creature? dealer,
		CardModel? cardSource)
	{
		if (target != base.Owner?.Creature || dealer == null)
		{
			return amount;
		}

		// 检查攻击者是否拥有寒冷或渐冻状态
		bool hasCold = dealer.GetPower<ColdPower>() != null && dealer.GetPower<ColdPower>()!.Amount > 0;
		bool hasFrostbite = dealer.GetPower<FrostbitePower>() != null && dealer.GetPower<FrostbitePower>()!.Amount > 0;

		if (hasCold || hasFrostbite)
		{
			// 伤害降低 25%
			return decimal.Floor(amount * 0.75m);
		}

		return amount;
	}

	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}
}
