using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 煎蛋：获得 7（9）点防御。
/// 0 费，食物牌（技能类型 + 食物接口），token 普通稀有度，目标自身。
/// 由【蛋打】衍生生成，不会出现在卡池中。
/// </summary>
public sealed class RoastEgg : CardModel, IFoodCard
{
	// 获得格挡，便于机制识别
	public override bool GainsBlock => true;

	// 动态变量：基础格挡 7 点（升级后 9 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new BlockVar(7m, ValueProp.Move) };

	public RoastEgg()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后格挡 7 -> 9 (+2)
		base.DynamicVars.Block.UpgradeValueBy(2m);
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌中加入指定数量的【煎蛋】。
	/// </summary>
	public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, bool isUpgraded, ICombatState combatState)
	{
		Godot.GD.Print($"[RoastEgg.CreateInHand] count={count}, isUpgraded={isUpgraded}");
		if (count == 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}

		List<CardModel> eggs = new List<CardModel>();
		for (int i = 0; i < count; i++)
		{
			RoastEgg egg = combatState.CreateCard<RoastEgg>(owner);
			if (isUpgraded)
			{
				egg.UpgradeInternal();
			}
			eggs.Add(egg);
		}

		await CardPileCmd.AddGeneratedCardsToCombat(eggs, PileType.Hand, owner);
		return eggs;
	}
}
