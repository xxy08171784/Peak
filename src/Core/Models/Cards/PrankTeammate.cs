using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 恶搞队友：获得 2 费，给指定的队友造成 5（1）点伤害。
/// 0 费，技能牌，罕见稀有度，目标任意队友。
/// 多人专属卡牌。
/// </summary>
public sealed class PrankTeammate : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 卡面尚未绘制，暂用 beta 占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	// 动态变量：对队友造成伤害 5（升级后 1）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(5m, ValueProp.Move)
	};

	public PrankTeammate()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 自己获得 2 费
		await PlayerCmd.GainEnergy(2m, base.Owner);

		// 2. 给队友造成 5（1）点伤害
		await CreatureCmd.Damage(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars.Damage.BaseValue,
			ValueProp.Move,
			base.Owner.Creature,
			this,
			cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 5 -> 1 (-4)
		base.DynamicVars.Damage.UpgradeValueBy(-4m);
	}
}
