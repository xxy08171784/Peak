using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 急行军：固有。获得急行军能力：每回合开始时，额外增加一次环境。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class RapidMarch : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/rapid_march.png");

	// 固有关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };

	// 动态变量：基础每回合额外增加 1 次环境
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<RapidMarchPower>(1m)
	};

	// 悬停提示：显示急行军能力说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<RapidMarchPower>()
	};

	public RapidMarch()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<RapidMarchPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["RapidMarchPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}
