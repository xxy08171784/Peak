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
/// 滋补：能力牌，获得滋补 Power。
/// 每当你打出一张食物牌，获得 1 层覆甲，1 点力量。
/// 2 费（升级后 1 费），能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Nourishing : CardModel
{
	// 动态变量：基础每次获得 1 层覆甲/1 点力量
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<NourishingPower>(1m)
	};

	// 悬停提示：显示滋补的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<NourishingPower>()
	};

	public Nourishing()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<NourishingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["NourishingPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1
		base.EnergyCost.UpgradeBy(-1);
	}
}
