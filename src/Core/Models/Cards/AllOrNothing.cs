using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 孤注一掷：对所有敌人造成 3x² 点伤害，x = 自身负面状态（debuff）种类数。
/// 2 费（升级后 1 费），攻击牌，稀有稀有度。
/// </summary>
public sealed class AllOrNothing : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：all_or_nothing.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/all_or_nothing.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/all_or_nothing.png");

	// 动态变量：计算伤害 = 0 + 3 * (debuff种类数)² = 3x²
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CalculationBaseVar(0m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
		{
			int debuffCount = card.Owner.Creature.Powers.Count(p => p.TypeForCurrentAmount == PowerType.Debuff);
			return (decimal)(debuffCount * debuffCount);
		})
	};

	public AllOrNothing()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 对所有敌人造成 3x² 点伤害（x = 自身 debuff 种类数）
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this, cardPlay)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1
		base.EnergyCost.UpgradeBy(-1);
	}
}
