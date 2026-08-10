using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

// 引入你的 HeatPower 命名空间

namespace peak.Core.Models.Cards;

/// <summary>
/// 辣椒：普通技能卡
/// 1费，消耗，获得 17(25) 点炎热值
/// </summary>
public sealed class Chili : CardModel,IFoodCard
{
    
    // 固有关键字：消耗（Exhaust）
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

    // 悬停提示：侧边栏自动显示炎热值（HeatPower）说明框
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<HeatPower>() };

    // 将变量实例注册到卡牌变量池中（供本地化 JSON 渲染 {HeatPower:diff()}）
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<HeatPower>(17m) };

    public Chili()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 给玩家自己施加 17(25) 层炎热值
        await PowerCmd.Apply<HeatPower>(
            choiceContext, 
            base.Owner.Creature, 
            base.DynamicVars["HeatPower"].BaseValue, 
            base.Owner.Creature, 
            this
        );
    }

    // 升级后炎热值 17 -> 25 (+8)
    protected override void OnUpgrade()
    {
        base.DynamicVars["HeatPower"].UpgradeValueBy(8m);
    }
}