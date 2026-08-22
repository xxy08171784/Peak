using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 善意投喂：使一名队友获得 1 费，3 层中毒，自己获得 10 点格挡。
/// 1 费（升级后 0 费），技能牌，罕见稀有度，目标任意队友。
/// 多人专属卡牌。
/// </summary>
public sealed class KindFeeding : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 卡面尚未绘制，暂用 beta 占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		base.EnergyHoverTip,
		HoverTipFactory.FromPower<ZhongduPower>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(1),
		new PowerVar<ZhongduPower>(3m),
		new BlockVar(10m, ValueProp.Move)
	};

	public KindFeeding()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 队友获得 1 费
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, cardPlay.Target.Player!);

		// 2. 队友获得 3 层中毒
		await PowerCmd.Apply<ZhongduPower>(
			choiceContext, cardPlay.Target, base.DynamicVars["ZhongduPower"].BaseValue, base.Owner.Creature, this);

		// 3. 自己获得 10 点格挡
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay: null);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
