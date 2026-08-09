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
/// 每回合开始时获得 7（10）点炎热。
/// 1 费，能力牌，普通稀有度，目标自身。
/// </summary>
public sealed class WarmPaste : CardModel
{
	// 动态变量：基础每回合获得 7 点炎热值（升级后 10 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<WarmPastePower>(7m)
	};

	// 悬停提示：显示暖宝宝的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<WarmPastePower>()
	};

	public WarmPaste()
		: base(1, CardType.Power, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
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
		// 升级后每回合获得炎热值从 7 提升到 10 (+3)
		base.DynamicVars["WarmPastePower"].UpgradeValueBy(3m);
	}
}
