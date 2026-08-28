using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 凌驾：获得 3（4）点能量，抽 3 张牌，获得 4 层覆甲。
/// 0 费，能力牌，先古稀有度，目标自身。未升级无固有，升级后获得固有。
/// </summary>
public sealed class Override : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/override.png");

	// 动态变量：基础能量 3、抽牌 3、覆甲 4
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(3),
		new CardsVar(3),
		new PowerVar<PlatingPower>(4m)
	};

	// 悬停提示：显示覆甲机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		base.EnergyHoverTip,
		HoverTipFactory.FromPower<PlatingPower>()
	};

	public Override()
		: base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 3（4）点能量
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);

		// 2. 抽 3 张牌
		await CardPileCmd.Draw(
			choiceContext,
			base.DynamicVars["Cards"].BaseValue,
			base.Owner
		);

		// 3. 获得 4 层覆甲
		await PowerCmd.Apply<PlatingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["PlatingPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后能量 3 -> 4 (+1)
		base.DynamicVars["Energy"].UpgradeValueBy(1m);

		// 升级后新增固有词条
		AddKeyword(CardKeyword.Innate);
	}
}
