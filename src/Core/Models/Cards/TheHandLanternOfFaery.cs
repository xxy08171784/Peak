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
/// 仙子提灯：能力牌，获得仙子提灯 Power。
/// 4（5）回合内，当你的回合结束时，回复 2 生命，降低 10 炎热，降低 5 中毒。
/// 1 费，能力牌，稀有稀有度，目标自身。
/// </summary>
public sealed class TheHandLanternOfFaery : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：the_hand_lantern_of_faery.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/the_hand_lantern_of_faery.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/the_hand_lantern_of_faery.png");

	// 动态变量：基础持续 4 回合（升级后 5 回合）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<TheHandLanternOfFaeryPower>(4m)
	};

	// 悬停提示：显示仙子提灯的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<TheHandLanternOfFaeryPower>()
	};

	public TheHandLanternOfFaery()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<TheHandLanternOfFaeryPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["TheHandLanternOfFaeryPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后持续 4 -> 5 回合 (+1)
		base.DynamicVars["TheHandLanternOfFaeryPower"].UpgradeValueBy(1m);
	}
}
