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
/// 暖宝宝：能力牌，获得暖宝宝 Power。
/// 立即获得 9（13）点炎热，每回合开始时获得 6（9）点炎热。
/// 1 费，能力牌，普通稀有度，目标自身。
/// </summary>
public sealed class WarmPaste : CardModel, IItemCard
{
	// 动态变量：基础每回合获得 6 点炎热值（升级后 9 点）；立即获得 9 点（13）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<WarmPastePower>(6m),
		new PowerVar<HeatPower>(9m)
	};

	// 悬停提示：显示暖宝宝的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<WarmPastePower>(),
		HoverTipFactory.FromPower<HeatPower>()
	};

	public WarmPaste()
		: base(1, CardType.Power, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 立即获得 9（13）点炎热
		decimal upfrontHeat = base.DynamicVars["HeatPower"].BaseValue;
		await PowerCmd.Apply<HeatPower>(
			choiceContext,
			base.Owner.Creature,
			upfrontHeat,
			base.Owner.Creature,
			this
		);

		// 2. 获得暖宝宝 Power（每回合开始时获得 6/9 炎热）
		await PowerCmd.Apply<WarmPastePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["WarmPastePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每回合获得炎热值从 6 提升到 9 (+3)
		base.DynamicVars["WarmPastePower"].UpgradeValueBy(3m);
		// 升级后立即获得炎热值从 9 提升到 13 (+4)
		base.DynamicVars["HeatPower"].UpgradeValueBy(4m);
	}
}
