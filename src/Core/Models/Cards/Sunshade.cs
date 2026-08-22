using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 遮阳伞：能力牌，获得遮阳伞 Power。
/// 立即失去至多 10（15）点炎热，每回合结束时额外失去至多 35%（50%）炎热值。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Sunshade : CardModel, IItemCard
{
	// 动态变量：基础每回合额外失去 35% 炎热值（升级后 50%）；立即失去 10 点炎热（15）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SunshadePower>(35m),
		new PowerVar<HeatPower>(10m)
	};

	// 悬停提示：显示遮阳伞的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SunshadePower>()
	};

	public Sunshade()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 立即失去至多 10（15）点炎热
		HeatPower? heat = base.Owner.Creature.GetPower<HeatPower>();
		if (heat != null && heat.Amount > 0)
		{
			int upfrontLoss = (int)base.DynamicVars["HeatPower"].BaseValue;
			int toLose = (int)System.Math.Min(heat.Amount, upfrontLoss);
			await PowerCmd.ModifyAmount(choiceContext, heat, -toLose, base.Owner.Creature, this);
		}

		// 2. 获得遮阳伞 Power（每回合结束时额外失去 35%/50% 炎热值）
		await PowerCmd.Apply<SunshadePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SunshadePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后百分比从 35 提升到 50 (+15)
		base.DynamicVars["SunshadePower"].UpgradeValueBy(15m);
		// 升级后立即失去炎热值从 10 提升到 15 (+5)
		base.DynamicVars["HeatPower"].UpgradeValueBy(5m);
	}
}
