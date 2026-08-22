using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 蘑菇4：给自己 4 层孢子，给自己 1 层虚弱和 1 层脆弱。
/// 1 费（升级后 0 费），token 稀有度，目标自身，消耗（保留），食物牌。
/// 由【蘑菇盲盒】随机生成，不会出现在卡池中。
/// </summary>
public sealed class Mushroom4 : CardModel, IFoodCard
{
	// 卡面图片（文件名与卡牌 ID 不一致，需显式指定）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/mushroom_4.png");

	

	// 消耗 + 保留关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, CardKeyword.Retain };

	// 悬停提示：按卡面顺序显示全部状态说明。
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SporePower>(),
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<FrailPower>()
	};

	// 动态变量：孢子 4 层、虚弱 1 层、脆弱 1 层。
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SporePower>(4m),
		new PowerVar<WeakPower>(1m),
		new PowerVar<FrailPower>(1m)
	};

	public Mushroom4()
		: base(1, CardType.Skill, CardRarity.Token, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 给自己 4 层孢子
		await PowerCmd.Apply<SporePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SporePower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 给自己 1 层虚弱
		await PowerCmd.Apply<WeakPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["WeakPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 3. 给自己 1 层脆弱
		await PowerCmd.Apply<FrailPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["FrailPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 -1（1 费 → 0 费）
		EnergyCost.UpgradeBy(-1);
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌中加入指定数量的【蘑菇4】。
	/// </summary>
	public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, bool isUpgraded, ICombatState combatState)
	{
		if (count == 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}

		List<CardModel> mushrooms = new List<CardModel>();
		for (int i = 0; i < count; i++)
		{
			Mushroom4 mushroom = combatState.CreateCard<Mushroom4>(owner);
			if (isUpgraded)
			{
				mushroom.UpgradeInternal();
			}
			mushrooms.Add(mushroom);
		}

		await CardPileCmd.AddGeneratedCardsToCombat(mushrooms, PileType.Hand, owner);
		return mushrooms;
	}
}
