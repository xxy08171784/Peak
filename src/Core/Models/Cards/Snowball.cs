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
/// 可被【滚雪球】多次升级：
/// - 升级 n 次：给予 (n+1) 层寒冷、n 层易伤、n 层虚弱（n ≥ 1）。
/// </summary>
public sealed class Snowball : CardModel
{
	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 可多次升级（由滚雪球逐级强化）
	public override int MaxUpgradeLevel => 99;

	// 悬停预览
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ColdPower>(),
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>()
	};

	// 动态变量：基础 1 层寒冷、1 层虚弱、0 层易伤（可被滚雪球多次升级）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ColdPower>("Cold", 1m),
		new PowerVar<VulnerablePower>("Vulnerable", 0m),
		new PowerVar<WeakPower>("Weak", 1m)
	};

	public Snowball()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 按升级层数计算各数值
		int level = CurrentUpgradeLevel;
		decimal cold = level + 1m;
		decimal vulnerable = level >= 1 ? (decimal)level : 0m;
		decimal weak = level >= 1 ? (decimal)level : 1m;

		// 给予寒冷
		if (cold > 0m)
		{
			await PowerCmd.Apply<ColdPower>(
				choiceContext,
				cardPlay.Target,
				cold,
				base.Owner.Creature,
				this
			);
		}

		// 给予易伤
		if (vulnerable > 0m)
		{
			await PowerCmd.Apply<VulnerablePower>(
				choiceContext,
				cardPlay.Target,
				vulnerable,
				base.Owner.Creature,
				this
			);
		}

		// 给予虚弱
		if (weak > 0m)
		{
			await PowerCmd.Apply<WeakPower>(
				choiceContext,
				cardPlay.Target,
				weak,
				base.Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后根据新层数设置各数值（n = 升级后的层数）
		int level = CurrentUpgradeLevel;
		base.DynamicVars["Cold"].BaseValue = level + 1m;
		base.DynamicVars["Vulnerable"].BaseValue = level >= 1 ? level : 0m;
		base.DynamicVars["Weak"].BaseValue = level >= 1 ? level : 1m;
	}
}
