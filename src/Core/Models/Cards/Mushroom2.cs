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
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 蘑菇2：给自己 4 层孢子，对所有敌人造成 9（12）点伤害。
/// 1 费，token 稀有度，目标自身，消耗，食物牌。
/// 由【蘑菇盲盒】随机生成，不会出现在卡池中。
/// </summary>
public sealed class Mushroom2 : CardModel, IFoodCard
{
	// 卡面图片（文件名与卡牌 ID 不一致，需显式指定）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/mushroom_2.png");

	

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 悬停提示：显示孢子的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SporePower>()
	};

	// 动态变量：孢子 4 层、伤害 9 点（升级后 12 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SporePower>(4m),
		new DamageVar(9m, ValueProp.Move)
	};

	public Mushroom2()
		: base(1, CardType.Attack, CardRarity.Token, TargetType.AllEnemies)
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

		// 2. 对所有敌人造成 9（12）点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 9 -> 12 (+3)
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌中加入指定数量的【蘑菇2】。
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
			Mushroom2 mushroom = combatState.CreateCard<Mushroom2>(owner);
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
