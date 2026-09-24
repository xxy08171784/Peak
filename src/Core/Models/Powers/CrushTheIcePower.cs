using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 碎冰：自己（含宠物）对拥有渐冻的敌人打出的攻击伤害提高（每层提高 50%/75%）。
/// Amount 即增伤百分比，多张碎冰可叠加。
///
/// 使用 override ModifyDamageMultiplicative 实现增伤，
/// 类似 VulnerablePower（易伤）的增伤机制 —— 判定条件也跟易伤对齐：
///   1) 目标是"带渐冻的敌人"；
///   2) 伤害来源属于碎冰持有者自己（含其宠物），队友 / 敌人 / 环境来源不享受；
///   3) 只放大攻击伤害（ValueProp.Move 且非 Unpowered），
///      炎热、中毒、由能力牌/遗物/药水产生的伤害都不吃碎冰。
/// </summary>
public sealed class 
	CrushTheIcePower : PowerModel
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
	/// 乘算增伤钩子：只放大"自己（含宠物）打出的攻击伤害"，且目标是带渐冻的敌人。
	/// 参考 VulnerablePower（易伤）的实现方式。
	/// </summary>
	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		// 只对拥有渐冻的目标生效
		if (target == null || target.GetPower<FrostbitePower>() == null)
		{
			return 1m;
		}

		// 只对"自己这边"打出的伤害生效：队友 / 敌人 / 环境（dealer 为 null）都不享受这个增益。
		// 宠物（Osty）由自己带，算自己的伤害。
		if (dealer == null)
		{
			return 1m;
		}
		bool fromMySide = dealer == Owner
			|| (Owner.Player != null && dealer.PetOwner == Owner.Player);
		if (!fromMySide)
		{
			return 1m;
		}

		// 只放大攻击伤害：炎热、中毒、以及由能力牌 / 遗物 / 药水产生的伤害都不是 ValueProp.Move，
		// 一律不吃碎冰（与官方 VulnerablePower 的 IsPoweredAttack() 过滤一致）。
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}

		Flash(); // 碎冰图标闪烁，提示玩家触发了增伤

		return DamageMultiplier;
	}
}
