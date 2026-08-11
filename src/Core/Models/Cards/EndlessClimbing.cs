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
/// 无尽攀登：获得 3（4）X 层覆甲。
/// X 费（投入全部能量），技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class EndlessClimbing : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/endless_climbing.png");

	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<PlatingPower>(3m)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<PlatingPower>()
	};

	public EndlessClimbing()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int xValue = ResolveEnergyXValue();
		if (xValue <= 0) return;

		await PowerCmd.Apply<PlatingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["PlatingPower"].BaseValue * xValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每 X 获得 3 -> 4 层覆甲
		base.DynamicVars["PlatingPower"].UpgradeValueBy(1m);
	}
}