using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 冷库：拥有渐冻的敌人造成的伤害降低 25%。
/// 稀有稀有度。
/// 参考 WeakPower（虚弱）的实现，使用 ModifyDamageMultiplicative 乘算减伤。
/// 返回 0.75m 使敌人造成的伤害 ×0.75。
/// </summary>
public sealed class ColdStorage : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	/// <summary>
	/// 减伤乘算钩子：攻击者拥有渐冻时伤害 ×0.75。
	/// 类似 WeakPower 的 ModifyDamageMultiplicative，但条件为攻击者有 FrostbitePower。
	/// </summary>
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		// 只对宿主受到的伤害生效
		if (target != base.Owner?.Creature)
		{
			return 1m;
		}

		// 检查攻击者是否拥有渐冻
		if (dealer == null || dealer.GetPower<FrostbitePower>() == null)
		{
			return 1m;
		}

		Flash();
		return 0.75m; // 伤害降低 25%
	}
}
