using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 喇叭：抽 1 张牌，获得 1 费，每打出一次这张牌增加 1 费。
/// 0 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Trumpet : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：trumpet.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/trumpet.png");

	

	// 动态变量：基础抽 1 张牌、获得 1 费
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CardsVar(1),
		new EnergyVar(1)
	};

	public Trumpet()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 抽 1 张牌
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);

		// 2. 获得 1 费
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);

		// 3. 每打出一次这张牌，本场战斗费用 +1
		base.EnergyCost.AddThisCombat(1);
	}

	protected override void OnUpgrade()
	{
		// 无升级效果（费用递增机制不变）
	}
}
