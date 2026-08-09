using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 香蕉皮：造成 5 点伤害，给予 2 层虚弱。
/// 1 费，攻击牌，普通稀有度，单体敌人目标。
/// </summary>
public sealed class BananaSkin : CardModel
{
	// 动态变量：基础伤害 5 点、基础虚弱 2 层
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(5m, ValueProp.Move),
		new PowerVar<WeakPower>(2m)
	};

	public BananaSkin()
		: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 造成 5 点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		// 2. 给予 2 层虚弱
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
		// 升级后伤害不变（表格里香蕉皮只有基础值）
	}
}
