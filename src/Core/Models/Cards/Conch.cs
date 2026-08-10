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
/// 海螺：造成 12（16）点伤害。
/// 1 费，攻击牌，普通稀有度，单体敌人目标。
/// </summary>
public sealed class Conch : CardModel
{
	// 动态变量：基础伤害 12 点
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DamageVar(12m, ValueProp.Move) };

	public Conch()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 +4（12 → 16）
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
