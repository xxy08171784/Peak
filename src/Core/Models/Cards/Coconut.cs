using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

public sealed class Coconut : CardModel
{
    // 侧边栏悬停预览：显示【半块椰子】（若主卡已升级，预览也动态展示升级版）
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new IHoverTip[] { HoverTipFactory.FromCard<HalfCoconut>(base.IsUpgraded) };

    // 基础变量：10点攻击伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        new DynamicVar[] { new DamageVar(10m, ValueProp.Move) };

    public Coconut()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1. 造成 10 点伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 2. 生成 2 张【半块椰子】加入手牌（主卡若升级，衍生卡也升级）
        await HalfCoconut.CreateInHand(base.Owner, 2, base.IsUpgraded, base.CombatState);
    }

    // 升级逻辑：伤害 10 -> 12 (+2)
    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
