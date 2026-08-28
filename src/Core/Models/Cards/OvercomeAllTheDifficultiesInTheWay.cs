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
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

/// <summary>
/// 过关斩将：对所有敌人造成 5（7）× 场景切换次数 点伤害。
/// 1 费，攻击牌，稀有稀有度，目标所有敌人。
/// </summary>
public sealed class OvercomeAllTheDifficultiesInTheWay : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/overcome_all_the_difficulties_in_the_way.png");

	// 动态变量：基础伤害 5（升级后 7），每切换一次场景造成一次伤害
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CalculationBaseVar(0m),
		new ExtraDamageVar(5m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
		{
			MyClimbing? myClimbing = card.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
			return myClimbing?.TotalEnvironmentSwitches ?? 0;
		})
	};

	public OvercomeAllTheDifficultiesInTheWay()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 对所有敌人造成 5（7）× 场景切换次数 点伤害
		await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
			.FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后基础伤害 5 -> 7 (+2)
		base.DynamicVars["ExtraDamage"].UpgradeValueBy(2m);
	}
}
