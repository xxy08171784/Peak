using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 初始物资：获得初始物资能力：每当你回到 0 海岛时，获得一张棉花糖。
/// 1 费（升级后 0 费），能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class InitialSupplies : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/initial_supplies.png");

	// 动态变量：基础 1 层能力
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<InitialSuppliesPower>(1m)
	};

	// 悬停提示：显示初始物资能力说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<InitialSuppliesPower>()
	};

	public InitialSupplies()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<InitialSuppliesPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["InitialSuppliesPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}
