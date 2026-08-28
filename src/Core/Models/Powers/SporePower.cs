using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

    /// <summary>
    /// 获得孢子时触发过载判定：三 buff 总和 ≥ 100 时判定（孢子最多则失去30孢子并接管回合）。
    /// 失去孢子（amount &lt; 0）不触发，避免死循环。
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this)
        {
            return;
        }
        if (amount > 0)
        {
            await BuffOverloadChecker.TryTrigger(Owner, choiceContext);
        }
    }
}