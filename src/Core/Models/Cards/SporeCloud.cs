using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 孢子云：0 费技能卡，罕见。
/// 获得 10 层孢子，给予一名（升级后所有）敌人 2 层易伤。
/// 目标类型固定为单体敌人；升级后在 OnPlay 中改为对全体敌人施加易伤。
/// </summary>
public sealed class SporeCloud : CardModel
{
    // 悬浮提示：显示“孢子”和“易伤”的效果说明
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SporePower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    };

    // 动态变量：10 层孢子、2 层易伤
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SporePower>(10m),
        new PowerVar<VulnerablePower>(2m)
    };

    // 构造函数：0 费，技能卡，罕见，基础目标为单体敌人
    public SporeCloud()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    // 升级后目标变为所有敌人（和 ShareMisfortune 同款模式）
    public override TargetType TargetType => 
        base.IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // PowerVar<T> 默认键名 = C# 类名（SporePower / VulnerablePower）
        decimal sporeAmount = base.DynamicVars["SporePower"].BaseValue;
        decimal vulnAmount = base.DynamicVars.Vulnerable.BaseValue;

        // 1. 玩家自己获得 10 层孢子
        await PowerCmd.Apply<SporePower>(
            choiceContext,
            base.Owner.Creature,
            sporeAmount,
            base.Owner.Creature,
            this
        );

        // 2. 给予敌人易伤（未升级：单体；升级后：全体，用 IEnumerable 重载）
        if (base.IsUpgraded)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                base.CombatState.HittableEnemies,   // ← 全体敌人
                vulnAmount,
                base.Owner.Creature,
                this
            );
        }
        else
        {
            if (cardPlay.Target != null)
            {
                await PowerCmd.Apply<VulnerablePower>(
                    choiceContext,
                    cardPlay.Target,
                    vulnAmount,
                    base.Owner.Creature,
                    this
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级主要改变了作用范围（OnPlay 内处理），数值无需改动
    }
}