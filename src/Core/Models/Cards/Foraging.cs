using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 觅食：将一个随机食物卡添加进你的手牌。
/// 1 费（升级后 0 费），技能牌，罕见稀有度，目标自身，消耗。
/// </summary>
public sealed class Foraging : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：foraging.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/foraging.png");

	

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 食物池：所有已设计的食物卡
	private static readonly Type[] FoodPool = new Type[]
	{
		typeof(Banana),
		typeof(Chili),
		typeof(EnergyDrink),
		typeof(GoodnightBerry),
		typeof(GreenBerry),
		typeof(HalfCoconut),
		typeof(IceDew),
		typeof(Lollipop),
		typeof(Marshmallow),
		typeof(MedicinalRootstock),
		typeof(MixedNuts),
		typeof(Mushroom1),
		typeof(Mushroom2),
		typeof(Mushroom3),
		typeof(Mushroom4),
		typeof(Mushroom5),
		typeof(PandoraBox),
		typeof(PoisonTrumpetMushroom),
		typeof(RoastChicken),
		typeof(RoastEgg),
	};

	public Foraging()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 从食物池中随机选择一张
		int index = base.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, FoodPool.Length);
		Type foodType = FoodPool[index];
		Godot.GD.Print($"[Foraging] roll={index}, food={foodType.Name}");

		// 从 ModelDb 获取该卡牌的 canonical 实例（不能直接 new，会触发 DuplicateModelException）
		CardModel? canonical = null;
		var cardMethod = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes);
		if (cardMethod != null)
		{
			canonical = (CardModel?)cardMethod.MakeGenericMethod(foodType).Invoke(null, null);
		}
		if (canonical == null)
		{
			Godot.GD.Print($"[Foraging] failed to get canonical card {foodType.Name}");
			return;
		}

		// 创建战斗副本（可变的）
		CardModel food = base.CombatState.CreateCard(canonical, base.Owner);

		// 若主卡已升级，则生成的食物卡也升级
		if (base.IsUpgraded)
		{
			food.UpgradeInternal();
		}

		// 加入手牌
		await CardPileCmd.AddGeneratedCardsToCombat(new[] { food }, PileType.Hand, base.Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
