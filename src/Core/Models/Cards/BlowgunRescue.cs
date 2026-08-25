using System;
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
/// 吹箭救援：消耗，指定一名队友，给予 1 层无实体，5 层再生，结束他的回合。
/// 2 费，技能牌，稀有稀有度，目标任意队友。
/// 多人专属卡牌。
/// </summary>
public sealed class BlowgunRescue : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<IntangiblePower>(),
		HoverTipFactory.FromPower<RegenPower>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<IntangiblePower>(1m),
		new PowerVar<RegenPower>(5m)
	};

	// 卡面图片（文件名与卡牌 ID 一致：BLOWGUN_RESCUE.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/BLOWGUN_RESCUE.png");

	public BlowgunRescue()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
	{
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1
		base.EnergyCost.UpgradeBy(-1);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 给予 1 层无实体
		await PowerCmd.Apply<IntangiblePower>(
			choiceContext, cardPlay.Target, base.DynamicVars["IntangiblePower"].BaseValue, base.Owner.Creature, this);

		// 2. 给予 5 层再生
		await PowerCmd.Apply<RegenPower>(
			choiceContext, cardPlay.Target, base.DynamicVars["RegenPower"].BaseValue, base.Owner.Creature, this);

		// 3. 结束队友的回合
		PlayerCmd.EndTurn(cardPlay.Target.Player!, canBackOut: false);
	}
}
