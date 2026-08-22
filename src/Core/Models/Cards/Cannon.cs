using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 大炮：对所有敌人造成 12（15）点伤害。
/// 如果被[gold]烤[/gold]，会[gold]自动打出[/gold]：对所有敌人造成 24 点伤害，然后[gold]消耗[/gold]。
/// 2 费，攻击牌，罕见稀有度，目标所有敌人。
/// 大炮不是食物牌。【小烤】/【烤炉】烤它时，会触发其自动打出（AutoPlay）。
/// </summary>
public sealed class Cannon : CardModel, IItemCard
{
	// 卡面图片（文件名与卡牌 ID 一致：cannon.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/cannon.png");

	// 基础变量：12 点伤害（升级后 15 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new DamageVar(12m, ValueProp.Move)
	};

	public Cannon()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 被烤过：造成 24 点伤害。消耗由 GetResultPileTypeAndPositionForCardPlay
		// 重写返回 Exhaust 自动处理（带正确视觉 tween）。
		if (RoastTracker.WasRoasted(this))
		{
			await DamageCmd.Attack(24m)
				.FromCard(this, cardPlay)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
				.Execute(choiceContext);
			return;
		}

		// 未被烤：造成 12（15）点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	/// <summary>
	/// 被烤过的大炮打完自动进入消耗堆（而非弃牌堆）。
	/// </summary>
	protected override CardLocation GetResultLocationForCardPlay()
	{
		CardLocation result = base.GetResultLocationForCardPlay();
		if (RoastTracker.WasRoasted(this))
		{
			result.pileType = PileType.Exhaust;
		}
		return result;
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 12 -> 15 (+3)
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
