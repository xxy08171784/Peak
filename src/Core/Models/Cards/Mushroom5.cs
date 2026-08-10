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
/// 蘑菇5：给自己 4（0）层孢子，获得 2 点费用。
/// 1 费，token 稀有度，目标自身，消耗，食物牌。
/// 由【蘑菇盲盒】随机生成，不会出现在卡池中。
/// </summary>
public sealed class Mushroom5 : CardModel, IFoodCard
{
	// 卡面图片（文件名与卡牌 ID 不一致，需显式指定）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/mushroom_5.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/mushroom_5.png");

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 悬停提示：显示孢子的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SporePower>()
	};

	// 动态变量：基础孢子 4 层（升级后 0 层）、能量 2 点
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SporePower>(4m),
		new EnergyVar(2)
	};

	public Mushroom5()
		: base(1, CardType.Skill, CardRarity.Token, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 给自己 4（0）层孢子（升级后不再获得孢子）
		if (!base.IsUpgraded)
		{
			await PowerCmd.Apply<SporePower>(
				choiceContext,
				base.Owner.Creature,
				base.DynamicVars["SporePower"].BaseValue,
				base.Owner.Creature,
				this
			);
		}

		// 2. 获得 2 点费用
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级后孢子 4 -> 0（不再获得孢子）
		base.DynamicVars["SporePower"].UpgradeValueBy(-4m);
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌中加入指定数量的【蘑菇5】。
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
			Mushroom5 mushroom = combatState.CreateCard<Mushroom5>(owner);
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
