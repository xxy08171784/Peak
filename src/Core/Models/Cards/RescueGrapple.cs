using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 救援抓勾：抽 3（4）张牌，下回合开始时额外抽 3 张牌。
/// 1 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class RescueGrapple : CardModel, IItemCard
{
	// 卡面图片（文件名与卡牌 ID 一致：rescue_grapple.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/rescue_grapple.png");

	

	// 动态变量：基础抽牌数 3 张（升级后 4 张）、下回合额外抽 3 张
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(3),
		new PowerVar<DrawCardsNextTurnPower>(3m)
	};

	// 悬停提示：显示下回合额外抽牌的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<DrawCardsNextTurnPower>()
	};

	public RescueGrapple()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 抽 3（4）张牌
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);

		// 2. 获得下回合开始时额外抽 3 张牌的 power
		await PowerCmd.Apply<DrawCardsNextTurnPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["DrawCardsNextTurnPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后抽牌数 3 -> 4 (+1)
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
