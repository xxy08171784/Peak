using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 破碎打击：对指定敌人造成 10（15）× 渐冻层数 点伤害。
/// 1 费，攻击牌，稀有稀有度，目标任意敌人。
/// </summary>
public sealed class FragmentationStrike : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/fragmentation_strike.png");

	// 悬停预览：显示渐冻机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<FrostbitePower>()
	};

	// 动态变量：基础伤害 10（升级后 15），每层渐冻造成一次伤害
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CalculationBaseVar(0m),
		new ExtraDamageVar(10m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? target) =>
			target?.GetPowerAmount<FrostbitePower>() ?? 0)
	};

	public FragmentationStrike()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 造成 10（15）× 渐冻层数 点伤害
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后基础伤害 10 -> 15 (+5)
		base.DynamicVars["ExtraDamage"].UpgradeValueBy(5m);
	}
}
