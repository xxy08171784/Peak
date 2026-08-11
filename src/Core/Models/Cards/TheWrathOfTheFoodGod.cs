using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 食神之怒：造成 13 点伤害，你的卡组中每有一张食物牌额外增加 3（5）点伤害。
/// 2 费，攻击牌，罕见稀有度，目标任意敌人。
/// </summary>
public sealed class TheWrathOfTheFoodGod : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：the_wrath_of_the_food_god.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/the_wrath_of_the_food_god.png");

	

	// 动态变量：基础伤害 13 点，每张食物牌 +3 点（升级后 +5 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CalculationBaseVar(13m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
			PileType.Deck.GetPile(card.Owner).Cards.Count(c => c is IFoodCard))
	};

	public TheWrathOfTheFoodGod()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 造成动态计算的伤害：13 + 卡组中食物牌数量 * 3（5）
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后每张食物牌额外伤害 3 -> 5 (+2)
		base.DynamicVars["ExtraDamage"].UpgradeValueBy(2m);
	}
}
