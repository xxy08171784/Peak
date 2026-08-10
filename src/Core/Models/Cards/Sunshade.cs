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
/// 每回合结束时额外失去 9（12）点炎热。
/// 1 费，能力牌，普通稀有度，目标自身。
/// </summary>
public sealed class Sunshade : CardModel
{
	// 动态变量：基础每回合额外失去 9 点炎热值（升级后 12 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SunshadePower>(9m)
	};

	// 悬停提示：显示遮阳伞的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SunshadePower>()
	};

	public Sunshade()
		: base(1, CardType.Power, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
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
		// 升级后每回合额外失去炎热值从 9 提升到 12 (+3)
		base.DynamicVars["SunshadePower"].UpgradeValueBy(3m);
	}
}
