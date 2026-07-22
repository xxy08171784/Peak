using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 寒冷：一种不会自然流失的减益印记。
/// 当积攒到 2 层或更多时，会消耗 2 层并转化为 1层易伤、1层虚弱和 1层渐冻。
/// </summary>
public sealed class ColdPower : PowerModel
{
    // 寒冷属于 Debuff
    public override PowerType Type => PowerType.Debuff;

    // 使用层数堆叠
    public override PowerStackType StackType => PowerStackType.Counter;

    // 不重写 AfterSideTurnEnd 意味着它不会随着回合结束而自然减少

    /// <summary>
    /// 当宿主身上的状态层数发生改变后触发自我检查。
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, 
        PowerModel power, 
        decimal amount, 
        Creature? applier, 
        CardModel? cardSource)
    {
        // 安全拦截：只在改变的状态是“我自身”的时候才运行逻辑
        if (power != this)
        {
            return;
        }

        // 使用 while 循环：防止一次性获得 4 层或更多寒冷时，无法连续触发转化
        // 添加安全计数器防止死循环（最大迭代 100 次，远超正常游戏场景）
        int safetyCounter = 0;
        while (Amount >= 2 && safetyCounter++ < 100)
        {
            Flash(); // 状态图标闪烁，提示玩家触发了转化效果

            // 1. 扣除 2 层寒冷值自身
            await PowerCmd.ModifyAmount(choiceContext, this, -2, applier, cardSource);

            // 2. 施加 1 层易伤 (Vulnerable)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 1, applier, cardSource);

            // 3. 施加 1 层虚弱 (Weak)
            await PowerCmd.Apply<WeakPower>(choiceContext, Owner, 1, applier, cardSource);

            // 4. 施加 1 层渐冻 (Frostbite)
            await PowerCmd.Apply<FrostbitePower>(choiceContext, Owner, 1, applier, cardSource);
        }
    }
}