using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

public sealed class ClimacticAscent : CardModel
{
    // 基础消耗 1，技能卡，稀有度为 Rare，目标为自己
    public ClimacticAscent() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获取玩家的爬山遗物
        MyClimbing climbingRelic = Owner.Relics.OfType<MyClimbing>().FirstOrDefault();

        if (climbingRelic != null)
        {
            // 2. 切换到下一个环境状态（这会自动触发第 1 次切换效果）
            await climbingRelic.ModifyEnvironmentValue(choiceContext, 1);

            // 3. 获取切换后的新环境值
            int nextEnvValue = climbingRelic.DisplayAmount;

            // 4. 再额外强行触发 2 次新环境的效果（总共达成 3 次效果）
            for (int i = 0; i < 2; i++)
            {
                // 让遗物图标闪烁，表示效果正在连续重置触发
                climbingRelic.Flash();

                // 等待极短时间，保证视觉和命令队列的连贯性
                await Cmd.CustomScaledWait(0.2f, 0.3f);

                // 调用解耦后的效果执行方法
                await climbingRelic.ExecuteStateEffect(choiceContext, nextEnvValue);
            }
        }
        else
        {
            GD.Print("未找到 MyClimbing 遗物。");
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后消耗降为 0
        EnergyCost.UpgradeBy(-1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}