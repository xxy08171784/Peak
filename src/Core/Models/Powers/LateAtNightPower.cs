using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

public sealed class LateAtNightPower : PowerModel
{
    public const string OnlyEnemiesKey = "OnlyEnemies";

    public override PowerType Type => PowerType.Buff;

    // 可堆叠层数
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(OnlyEnemiesKey, 0m) // 0 = 所有人, 1 = 仅敌人
    };

    /// <summary>
    /// 严格匹配 STS2 官方 ShadowStepPower 的回合开始钩子写法
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 1. 检查当前回合的参与者中是否包含本能力的持有者
        if (!participants.Contains(base.Owner))
        {
            return;
        }

        Flash(); // 状态图标闪烁

        bool onlyEnemies = base.DynamicVars[OnlyEnemiesKey].BaseValue > 0m;
        decimal coldAmount = base.Amount; // 施加的寒冷层数 = 本能力当前的层数

        // 在回合开始的后台逻辑中，使用官方规范的 ThrowingPlayerChoiceContext
        var choiceContext = new ThrowingPlayerChoiceContext();

        if (!onlyEnemies)
        {
            // 2. 未升级状态：给所有友方（自己 + 队友 + 召唤物）施加寒冷
            foreach (var ally in combatState.PlayerCreatures)
            {
                if (ally.IsAlive)
                {
                    await PowerCmd.Apply<ColdPower>(choiceContext, ally, coldAmount, base.Owner, null);
                }
            }

            // 给所有敌人施加寒冷
            foreach (var enemy in combatState.HittableEnemies)
            {
                await PowerCmd.Apply<ColdPower>(choiceContext, enemy, coldAmount, base.Owner, null);
            }
        }
        else
        {
            // 3. 升级后状态：只给所有敌人施加寒冷
            foreach (var enemy in combatState.HittableEnemies)
            {
                await PowerCmd.Apply<ColdPower>(choiceContext, enemy, coldAmount, base.Owner, null);
            }
        }
    }
}