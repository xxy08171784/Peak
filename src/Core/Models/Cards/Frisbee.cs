using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 飞盘：造成 8（13）点伤害，给予 1 层易伤和 1 层虚弱。
/// 1 费，攻击牌，普通稀有度，单体敌人目标。
/// </summary>
public sealed class Frisbee : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>()
	};

	// 动态变量：基础伤害 8 点、基础易伤 1 层、基础虚弱 1 层
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
		new PowerVar<VulnerablePower>(1m),
		new PowerVar<WeakPower>(1m)
	};

	public Frisbee()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 造成 8 点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		// 2. 给予 1 层易伤
		await PowerCmd.Apply<VulnerablePower>(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars["VulnerablePower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 3. 给予 1 层虚弱
		await PowerCmd.Apply<WeakPower>(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars["WeakPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 8 → 13
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
