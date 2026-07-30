<<<<<<< HEAD
﻿using System.Collections.Generic;
=======
using System.Collections.Generic;
>>>>>>> origin/xxy
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

public sealed class MixedNuts : CardModel
{
<<<<<<< HEAD
    // 声明能量属性：基础获得 1 点能量
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(1) };

    // 可选：如果您希望这张卡像原版 Wisp 一样带有“消耗”关键字来维持游戏平衡，可以解开下方的注释：
     public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    // 浮动提示：当玩家悬停在卡牌上时，显示能量相关的规则提示
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { base.EnergyHoverTip };

    public MixedNuts()
        : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        // 0 费，技能牌，基础牌（Basic 对应开局自带），目标为自身
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 执行获得能量的指令
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级时：获得的能量值增加 1 点（总能量增益变为 2 点）
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
=======
	// 声明能量属性：基础获得 1 点能量
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(1) };

	// 可选：如果您希望这张卡像原版 Wisp 一样带有“消耗”关键字来维持游戏平衡，可以解开下方的注释：
	 public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 浮动提示：当玩家悬停在卡牌上时，显示能量相关的规则提示
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { base.EnergyHoverTip };

	public MixedNuts()
		: base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
		// 0 费，技能牌，基础牌（Basic 对应开局自带），目标为自身
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 执行获得能量的指令
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级时：获得的能量值增加 1 点（总能量增益变为 2 点）
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
>>>>>>> origin/xxy
