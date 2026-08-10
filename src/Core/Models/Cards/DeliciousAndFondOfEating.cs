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
/// 好吃爱吃继续吃：能力牌，获得好吃爱吃继续吃 Power。
/// 每当你打出一张食物牌，抽 1（2）张牌。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class DeliciousAndFondOfEating : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：delicious_and_fond_of_eating.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/delicious_and_fond_of_eating.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/delicious_and_fond_of_eating.png");

	// 动态变量：基础每次抽 1 张牌（升级后 2 张）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<DeliciousAndFondOfEatingPower>(1m)
	};

	// 悬停提示：显示好吃爱吃继续吃的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<DeliciousAndFondOfEatingPower>()
	};

	public DeliciousAndFondOfEating()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<DeliciousAndFondOfEatingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["DeliciousAndFondOfEatingPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后抽牌数 1 -> 2 (+1)
		base.DynamicVars["DeliciousAndFondOfEatingPower"].UpgradeValueBy(1m);
	}
}
