using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

/// <summary>
/// 雪甲：每当给予一层寒冷时，获得格挡（每层 2/3 点）。
/// Amount 即每层寒冷获得的格挡值，多张雪甲可叠加。
/// </summary>
public sealed class SnowArmorPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 每层寒冷获得的格挡值）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每给予一层寒冷获得的格挡值。
	/// </summary>
	public int BlockPerLayer => Amount;
}
