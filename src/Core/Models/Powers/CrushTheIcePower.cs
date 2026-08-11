using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 碎冰：对拥有渐冻的敌人造成的伤害提高（每层提高 50%/75%）。
/// Amount 即增伤百分比，多张碎冰可叠加。
/// </summary>
public sealed class CrushTheIcePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 增伤百分比，如 50 = 50% 增伤）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 伤害倍率 = 1 + Amount/100（如 Amount=50 → 1.5倍，Amount=75 → 1.75倍）
	/// </summary>
	public decimal DamageMultiplier => 1m + (decimal)Amount / 100m;

	/// <summary>
	/// 伤害修改钩子：对拥有渐冻的敌人造成额外伤害。
	/// </summary>
	public new decimal ModifyDamageGiven(
		ICombatState combatState,
		Creature target,
		decimal damage,
		CardModel? cardSource,
		Creature attacker)
	{
		// 只对拥有渐冻的目标生效
		if (target.GetPower<FrostbitePower>() == null)
		{
			return damage;
		}

		Flash(); // 碎冰图标闪烁，提示玩家触发了增伤

		return damage * DamageMultiplier;
	}
}
