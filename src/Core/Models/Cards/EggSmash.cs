using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 蛋打：造成 6（9）点伤害，90% 获得一张煎蛋，10% 获得烤鸡（火鸡）。
/// 1 费，攻击牌，罕见稀有度，目标任意敌人。
/// </summary>
public sealed class EggSmash : CardModel
{
	// 消耗关键词（升级前后都有）
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 侧边栏悬停预览：显示煎蛋与火鸡（若主卡已升级，预览也动态展示升级版）
	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		new IHoverTip[]
		{
			HoverTipFactory.FromCard<RoastEgg>(base.IsUpgraded),
			HoverTipFactory.FromCard<RoastChicken>(base.IsUpgraded)
		};

	// 基础变量：6点攻击伤害
	protected override IEnumerable<DynamicVar> CanonicalVars =>
		new DynamicVar[] { new DamageVar(6m, ValueProp.Move) };

	public EggSmash()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Godot.GD.Print($"[EggSmash] OnPlay start, IsUpgraded={base.IsUpgraded}");
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 造成 6 点伤害
		Godot.GD.Print($"[EggSmash] dealing damage {base.DynamicVars.Damage.BaseValue}");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);

		// 2. 90% 获得一张煎蛋，10% 获得一张火鸡（烤鸡），始终生成未升级版
		Godot.GD.Print($"[EggSmash] damage done, rolling for reward");
		double roll = base.Owner.RunState.Rng.CombatCardGeneration.NextDouble();
		Godot.GD.Print($"[EggSmash] roll={roll}, generating card");
		if (roll < 0.9)
		{
			await RoastEgg.CreateInHand(base.Owner, 1, false, base.CombatState);
		}
		else
		{
			await RoastChicken.CreateInHand(base.Owner, 1, false, base.CombatState);
		}
		Godot.GD.Print($"[EggSmash] done");
	}

	// 升级逻辑：伤害 6 -> 9 (+3)
	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
