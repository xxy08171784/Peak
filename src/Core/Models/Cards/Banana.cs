using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

public sealed class Banana : CardModel,IFoodCard
{
    // 规范：带治疗效果的卡牌，禁止在战斗中被随机发现/生成
    public override bool CanBeGeneratedInCombat => false;

    // 侧边栏悬停预览：显示【香蕉皮】（主卡升级时，预览也动态显示升级版香蕉皮）
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new IHoverTip[] { HoverTipFactory.FromCard<BananaPeel>(base.IsUpgraded) };

    // 基础变量：回复 4 点生命
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        new DynamicVar[] { new HealVar(4m) };

    public Banana()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 播放吃香蕉/施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 2. 回复 4（6）点生命值（参照 NotYet.cs 语法）
        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);

        // 3. 生成 1 张【香蕉皮】放入手牌（主卡升级时，香蕉皮也自动升级）
        await BananaPeel.CreateInHand(base.Owner, 1, base.IsUpgraded, base.CombatState);
    }

    // 升级效果：回复 4 -> 6 (+2)
    protected override void OnUpgrade()
    {
        base.DynamicVars.Heal.UpgradeValueBy(2m);
    }
}