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
/// 士气高涨：获得士气高涨能力：每当你切换环境时，获得 1 点覆甲并抽 1（升级后 2）张牌。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class HighMorale : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/high_morale.png");

	// 动态变量：基础 1 层（升级后 2 层→抽 2 张牌）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<HighMoralePower>(1m)
	};

	// 悬停提示：显示士气高涨能力说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<HighMoralePower>()
	};

	public HighMorale()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<HighMoralePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["HighMoralePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后 Amount 2 -> 3（覆甲 3、抽牌 2）
		base.DynamicVars["HighMoralePower"].UpgradeValueBy(1m);
	}
}
