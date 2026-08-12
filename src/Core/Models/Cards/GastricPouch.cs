using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 胃袋：造成 5（7）点伤害，你在本场战斗中每吃过一个食物，就额外打出一次。
/// 2 费，攻击牌，罕见稀有度，目标任意敌人。
///
/// 实现原理（参考原版"重放"机制）：重写 ModifyCardPlayCount，
/// 当此牌被打出时，根据本场战斗已打出的食物牌数量增加打出次数，
/// OnPlay 会在每次打出时各执行一次。
/// </summary>
public sealed class GastricPouch : CardModel
{
	private const string _calculatedHitsKey = "CalculatedHits";

	// 卡面图片（文件名与卡牌 ID 一致：gastric_pouch.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/gastric_pouch.png");

	// 动态变量：
	// 基础伤害 5 点（升级后 7 点）
	// 打出次数 = 1（基础）+ 1 × 本场战斗吃过的食物数
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(5m, ValueProp.Move),
		new CalculationBaseVar(1m),
		new CalculationExtraVar(1m),
		new CalculatedVar(_calculatedHitsKey).WithMultiplier((CardModel card, Creature? _) =>
			CountFoodEatenThisCombat(card))
	};

	// 悬停提示：显示"重放"机制的说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
	};

	public GastricPouch()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 造成 5（7）点伤害（每次打出执行一次）
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	/// <summary>
	/// 重放机制：本场战斗中每吃过一个食物，额外打出一次。
	/// 此方法在 OnPlayWrapper 的 GeneratePlayCount 阶段被 Hook 调用，
	/// 由于此牌正处于打出堆中（是 hook listener），因此会被调用。
	/// </summary>
	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card != this || card.Owner != base.Owner)
		{
			return playCount;
		}
		return playCount + CountFoodEatenThisCombat(card);
	}

	/// <summary>统计本场战斗中该玩家已打出的食物牌数量（含自动打出）。</summary>
	private static int CountFoodEatenThisCombat(CardModel card)
	{
		if (!CombatManager.Instance.IsInProgress || card.CombatState == null)
		{
			return 0;
		}
		return CombatManager.Instance.History.CardPlaysFinished.Count(e =>
			e.Actor == card.Owner.Creature && e.CardPlay.Card is IFoodCard);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 5 -> 7 (+2)
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
