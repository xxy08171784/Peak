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
/// 弹力菇：能力牌，获得弹力菇 Power。
/// 如果你在回合结束时没有任何格挡，获得 6（8）点格挡。
/// 1 费，能力牌，普通稀有度，目标自身。
/// </summary>
public sealed class ElasticMushroom : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：elastic_mushroom.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/elastic_mushroom.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/elastic_mushroom.png");

	// 动态变量：基础每次获得 6 点格挡（升级后 8 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<PleurotusEryngiiPower>(6m)
	};

	// 悬停提示：显示弹力菇的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<PleurotusEryngiiPower>()
	};

	public ElasticMushroom()
		: base(1, CardType.Power, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<PleurotusEryngiiPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["PleurotusEryngiiPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每次获得格挡 6 -> 8 (+2)
		base.DynamicVars["PleurotusEryngiiPower"].UpgradeValueBy(2m);
	}
}
