using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics; // 导入您自定义遗物所在的命名空间

namespace peak.Core.Models.Cards;

public sealed class Climbing : CardModel
{
    // 允许升级一次（0 -> 1）
    public override int MaxUpgradeLevel => 1;

    public Climbing()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        // 构造函数参数含义：基础能耗(1), 卡牌类型(Skill), 稀有度(Basic/初始), 目标类型(None/无目标)
    }

    /// <summary>
    /// 升级逻辑：降低 1 点能量消耗（1 -> 0）
    /// </summary>
    protected override void OnUpgrade()
    {
        // 根据 CardModel 中的规范，通过 UpgradeBy 调整能耗
        EnergyCost.UpgradeBy(-1);
    }

    /// <summary>
    /// 打出卡牌时的核心逻辑
    /// </summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner == null)
        {
            return;
        }

        // 在玩家已获得的遗物列表中查找您写的自定义遗物 "MyClimbing"
        MyClimbing climbingRelic = base.Owner.Relics.OfType<MyClimbing>().FirstOrDefault();

        if (climbingRelic != null)
        {
            // 如果找到了遗物，调用其公共方法，将环境值递增 1
            await climbingRelic.ModifyEnvironmentValue(choiceContext, 1);
        }
        else
        {
            // 兜底逻辑：如果玩家没有装备该遗物，可以选择不生效，或者触发一些视觉/音效提示
            SfxCmd.Play("event:/sfx/ui/relic_trigger_fail"); 
        }
    }
}