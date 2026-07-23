using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

public sealed class SporePower : PowerModel
{
    // 声明为 Debuff（减益效果），这会让它的层数文本颜色自动变为红色，且能受到“万能减益”等相关遗物/卡牌的加成
    public override PowerType Type => PowerType.Debuff;

    // 使用层数堆叠计数
    public override PowerStackType StackType => PowerStackType.Counter;

    // 不允许为负数，归零时会自动清除该状态
    public override bool AllowNegative => false;
}