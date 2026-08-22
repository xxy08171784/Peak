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
/// 挥汗如雨：能力牌，获得挥汗如雨 Power。
/// 每打出一张牌，失去至多 5（8）点炎热。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class SweatProfusely : CardModel
{
	// 动态变量：基础每张牌失去 5 点炎热值（升级后 8 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SweatProfuselyPower>(5m)
	};

	// 悬停提示：显示挥汗如雨的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SweatProfuselyPower>()
	};

	public SweatProfusely()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<SweatProfuselyPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SweatProfuselyPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每张牌失去炎热值从 5 提升到 8 (+3)
		base.DynamicVars["SweatProfuselyPower"].UpgradeValueBy(3m);
	}
}
