using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 晚安梅：消耗，获得 1 层无实体、3 点再生，然后结束你的回合。
/// 2 费（升级后 1 费），食物牌（技能类型 + 食物接口），稀有稀有度，目标自身。
/// </summary>
public sealed class GoodnightBerry : CardModel, IFoodCard
{
	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础无实体 1 层、基础再生 3 点
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<IntangiblePower>(1m),
		new PowerVar<RegenPower>(3m)
	};

	// 悬停提示：显示无实体与再生的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<IntangiblePower>(),
		HoverTipFactory.FromPower<RegenPower>()
	};

	public GoodnightBerry()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 1 层无实体
		await PowerCmd.Apply<IntangiblePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["IntangiblePower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 获得 3 点再生
		await PowerCmd.Apply<RegenPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["RegenPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 3. 结束你的回合
		PlayerCmd.EndTurn(base.Owner, canBackOut: false);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1
		base.EnergyCost.UpgradeBy(-1);
	}
}
