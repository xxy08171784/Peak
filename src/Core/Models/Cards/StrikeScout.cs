using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

public sealed class StrikeScout : CardModel
{
    // 标记卡牌拥有“打击”（Strike）标签，使卡牌能与相关遗物或其它卡牌产生联动
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    // 设定卡牌的初始属性，这里定义了基础伤害为 6 点
    // 注：模板中的 "global::_003C_003Ez__..." 是编译器生成的混淆类名，在实际 Mod 开发中，我们可以使用标准的 C# 集合（如数组或 List）来代替
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DamageVar(6m, ValueProp.Move) };

    public StrikeScout()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        // 1 费，攻击牌，基础稀有度，单体敌人目标
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 执行伤害指令。此处攻击特效使用了默认的斜劈 ("vfx/vfx_attack_slash")。
        // 如果您后续有了符合童子军题材的音效或视觉资源，可以修改 .WithHitFx(...) 中的路径
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash") 
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级后伤害增加 3 点（总伤害变为 9 点）
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}