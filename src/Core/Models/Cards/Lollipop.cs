using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 棒棒糖：所有手牌在本回合内变成0费，下回合开始时失去1（0）费。
/// 3 费（升级后 2 费），食物牌（技能类型 + 食物接口），稀有稀有度，目标自身，保留。
/// </summary>
public sealed class Lollipop : CardModel, IFoodCard
{
	// 保留关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	// 动态变量：基础疲劳 1 层（升级后 0 层，即不给予疲劳）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<TiredPower>(1m)
	};

	public Lollipop()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 所有手牌在本回合内变成0费
		foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards)
		{
			if (!card.EnergyCost.CostsX)
			{
				card.SetToFreeThisTurn();
			}
		}

		// 2. 给予自己疲劳 debuff（升级后为 0 层则不给予）
		decimal tiredAmount = base.DynamicVars["TiredPower"].BaseValue;
		if (tiredAmount > 0)
		{
			await PowerCmd.Apply<TiredPower>(
				choiceContext,
				base.Owner.Creature,
				tiredAmount,
				base.Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 3 → 2（-1）
		base.EnergyCost.UpgradeBy(-1);
		// 升级后疲劳层数降为 0（不给予疲劳 debuff）
		base.DynamicVars["TiredPower"].UpgradeValueBy(-1m);
	}
}