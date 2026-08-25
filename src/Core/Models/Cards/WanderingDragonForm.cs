using System;
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
/// 游龙形态：获得游龙形态 Power。
/// 每当你打出 1 张牌时，从多种效果中随机获得一个。
/// 3 费，能力牌，稀有稀有度，目标自身。
/// </summary>
public sealed class WanderingDragonForm : CardModel
{
	// 卡面尚未绘制，暂用占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	// 悬停提示：显示游龙形态的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<WanderingDragonFormPower>()
	};

	public WanderingDragonForm()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 获得游龙形态 Power（层数控制随机选取数量，默认 1）
		await PowerCmd.Apply<WanderingDragonFormPower>(
			choiceContext,
			base.Owner.Creature,
			1m,
			base.Owner.Creature,
			this
		);
	}
}