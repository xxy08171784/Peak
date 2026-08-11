using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 古老宝箱：失去 8（4）点生命值，将一个随机稀有道具或食物卡加入手牌。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class AncientChest : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：ancient_chest.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/ancient_chest.png");

	// 动态变量：基础失去 8 点生命值（升级后 4 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new HpLossVar(8m)
	};

	// 奖励池：稀有道具或食物卡
	private static readonly Type[] RewardPool = new Type[]
	{
		typeof(RescueGrapple),
		typeof(Panacea),
		typeof(CheckpointFlag),
		typeof(TheBookOfBones),
		typeof(LeaderTrumpet),
		typeof(TheHandLanternOfFaery),
		typeof(GoodnightBerry),
		typeof(PandoraBox),
		typeof(Lollipop),
	};

	public AncientChest()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 失去 8（4）点生命值（不可格挡、不吃力量加成）
		await CreatureCmd.Damage(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars.HpLoss.BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
			this,
			cardPlay
		);

		// 2. 从奖励池中随机选择一张
		int index = base.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, RewardPool.Length);
		Type rewardType = RewardPool[index];
		Godot.GD.Print($"[AncientChest] roll={index}, reward={rewardType.Name}");

		// 从 ModelDb 获取该卡牌的 canonical 实例（不能直接 new，会触发 DuplicateModelException）
		CardModel? canonical = null;
		var cardMethod = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes);
		if (cardMethod != null)
		{
			canonical = (CardModel?)cardMethod.MakeGenericMethod(rewardType).Invoke(null, null);
		}
		if (canonical == null)
		{
			Godot.GD.Print($"[AncientChest] failed to get canonical card {rewardType.Name}");
			return;
		}

		// 创建战斗副本（可变的）
		CardModel reward = base.CombatState.CreateCard(canonical, base.Owner);

		// 加入手牌
		await CardPileCmd.AddGeneratedCardsToCombat(new[] { reward }, PileType.Hand, base.Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级后失去生命值 8 -> 4 (-4)
		base.DynamicVars.HpLoss.UpgradeValueBy(-4m);
	}
}
