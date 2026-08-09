using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 棉花糖：罕见技能卡
/// 0费，保留，消耗，获得 1(2) 点能量
/// </summary>
public sealed class Marshmallow : CardModel,IFoodCard
{
    // 固有关键字：消耗（Exhaust） 与 保留（Retain）
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] 
    { 
        CardKeyword.Exhaust, 
        CardKeyword.Retain 
    };

    // 基础变量：获得 1 点能量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] 
    { 
        new EnergyVar(1) 
    };

    // 悬停提示：侧边栏自动显示能量（Energy）说明框
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] 
    { 
        base.EnergyHoverTip 
    };

    public Marshmallow()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得能量（参照 Wisp.cs 的官方标准写法）
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
    }

    // 升级逻辑：获得能量 1 -> 2 (+1)
    protected override void OnUpgrade()
    {
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }
}