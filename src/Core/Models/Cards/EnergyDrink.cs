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
/// 能量饮料：消耗，获得 2 层虚弱，获得 2（3）点敏捷。
/// 2 费，食物牌（技能类型 + 食物接口），罕见稀有度，目标自身。
/// </summary>
public sealed class EnergyDrink : CardModel, IFoodCard
{
	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础虚弱 2 层、基础敏捷 2 点（升级后 3 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<WeakPower>(2m),
		new PowerVar<DexterityPower>(2m)
	};

	public EnergyDrink()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 2 层虚弱
		await PowerCmd.Apply<WeakPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["WeakPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 获得 2（3）点敏捷
		await PowerCmd.Apply<DexterityPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["DexterityPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后敏捷 2 → 3
		base.DynamicVars["DexterityPower"].UpgradeValueBy(1m);
	}
}
