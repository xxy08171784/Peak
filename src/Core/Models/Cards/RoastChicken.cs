using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 火鸡：获得 6（8）层覆甲，获得 3 点生命上限。
/// 0 费，食物牌（技能类型 + 食物接口），token 稀有稀有度，目标自身。
/// 由【蛋打】衍生生成，不会出现在卡池中。
/// </summary>
public sealed class RoastChicken : CardModel, IFoodCard
{
	// 动态变量：基础覆甲 6 层（升级后 8 层）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<PlatingPower>(6m)
	};

	// 悬停提示：显示覆甲的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<PlatingPower>()
	};

	public RoastChicken()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 6 层覆甲
		await PowerCmd.Apply<PlatingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["PlatingPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 获得 3 点生命上限
		await CreatureCmd.GainMaxHp(base.Owner.Creature, 3m);
	}

	protected override void OnUpgrade()
	{
		// 升级后覆甲 6 -> 8 (+2)
		base.DynamicVars["PlatingPower"].UpgradeValueBy(2m);
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌中加入指定数量的【火鸡】。
	/// </summary>
	public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, bool isUpgraded, ICombatState combatState)
	{
		Godot.GD.Print($"[RoastChicken.CreateInHand] count={count}, isUpgraded={isUpgraded}");
		if (count == 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}

		List<CardModel> chickens = new List<CardModel>();
		for (int i = 0; i < count; i++)
		{
			RoastChicken chicken = combatState.CreateCard<RoastChicken>(owner);
			if (isUpgraded)
			{
				chicken.UpgradeInternal();
			}
			chickens.Add(chicken);
		}

		await CardPileCmd.AddGeneratedCardsToCombat(chickens, PileType.Hand, owner);
		return chickens;
	}
}
