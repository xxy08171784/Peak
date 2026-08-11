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
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 雪球：给予 1 层寒冷和 1 层虚弱。
/// 0 费，技能牌，token 稀有度，目标任意敌人，消耗。
/// 由滚雪球生成和升级，数值可变。
/// </summary>
public sealed class Snowball : CardModel
{
	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 悬停预览
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ColdPower>(),
		HoverTipFactory.FromPower<WeakPower>()
	};

	// 动态变量：基础 1 层寒冷、1 层虚弱（可被滚雪球增加）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ColdPower>(1m),
		new PowerVar<WeakPower>(1m)
	};

	public Snowball()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 给予 1 层寒冷
		await PowerCmd.Apply<ColdPower>(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars["ColdPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 给予 1 层虚弱
		await PowerCmd.Apply<WeakPower>(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars["WeakPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	/// <summary>
	/// 被滚雪球调用：将所有数值 +1（寒冷 +1，虚弱 +1）。
	/// </summary>
	public void AddAllValues(decimal amount)
	{
		base.DynamicVars["ColdPower"].BaseValue += amount;
		base.DynamicVars["WeakPower"].BaseValue += amount;
	}
}
