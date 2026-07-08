using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

/// <summary>
/// 渐冻：一种永久留存的标记性 Debuff。
/// 自身没有任何实际负面效果，但可以作为其他卡牌伤害加深的判定依据。
/// </summary>
public sealed class FrostbitePower : PowerModel
{
	// 属于 Debuff 类别
	public override PowerType Type => PowerType.Debuff;

	// 堆叠方式：层数无限累加
	public override PowerStackType StackType => PowerStackType.Counter;

	// 同样不重写任何倒计时清除代码，使其在战斗中永久保留
}
