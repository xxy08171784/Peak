using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;
using peak.Core.Models.Cards;

namespace peak.Core.Models.CardPools;

public sealed class ScoutCardPool : CardPoolModel
{
	public override string Title => "scout";

	public override string EnergyColorName => "scout";

	// 对应童子军卡牌的黄框材质路径
	public override string CardFrameMaterialPath => "card_frame_yellow";

	// 牌组界面中该职业卡牌条的代表色（亮黄色）
	public override Color DeckEntryCardColor => new Color("FFD700");

	// 能量指示器的轮廓颜色（暗黄/深金）
	public override Color EnergyOutlineColor => new Color("8B8000");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		// 这里应当返回童子军卡牌池中的所有卡牌。
		// 随着您设计并编写更多卡牌，只需在下方数组中添加对应的 ModelDb.Card<T>() 调用即可。
		return new CardModel[]
		{
			ModelDb.Card<StrikeScout>(),
			ModelDb.Card<DefendScout>(),
			ModelDb.Card<MixedNuts>(),
			ModelDb.Card<RockBolt>(),
			ModelDb.Card<Conch>(),
			ModelDb.Card<GreenBerry>(),
			ModelDb.Card<EnergyDrink>(),
			ModelDb.Card<Frisbee>(),
			ModelDb.Card<Coconut>(),
			ModelDb.Card<HalfCoconut>(),
			ModelDb.Card<Banana>(),
			ModelDb.Card<BananaPeel>(),
			ModelDb.Card<Marshmallow>(),
			ModelDb.Card<Chili>(),
			ModelDb.Card<Climb>(),
			ModelDb.Card<ReachThePeak>(),
			ModelDb.Card<ArsonExpert>(),
			ModelDb.Card<IceDew>(),
			ModelDb.Card<GoodnightBerry>(),
			ModelDb.Card<WarmPaste>(),
			ModelDb.Card<Sunshade>(),
			ModelDb.Card<TheBookOfBones>(),
			ModelDb.Card<LeaderTrumpet>(),
			ModelDb.Card<SweatProfusely>(),
			ModelDb.Card<Nourishing>(),
			ModelDb.Card<PoisonTrumpetMushroom>(),
			ModelDb.Card<EggSmash>(),
			ModelDb.Card<RoastEgg>(),
			ModelDb.Card<RoastChicken>(),
			ModelDb.Card<MedicinalRootstock>(),
			ModelDb.Card<Lollipop>(),
			ModelDb.Card<PandoraBox>(),
			ModelDb.Card<MushroomBoxSet>(),
			ModelDb.Card<Mushroom1>(),
			ModelDb.Card<Mushroom2>(),
			ModelDb.Card<Mushroom3>(),
			ModelDb.Card<Mushroom4>(),
			ModelDb.Card<Mushroom5>(),
			ModelDb.Card<Aftertaste>(),
			ModelDb.Card<SecretRecipeChiliSauce>(),
			ModelDb.Card<ImmuneToAllPoisons>(),
			ModelDb.Card<DeliciousAndFondOfEating>(),
			ModelDb.Card<Buffet>(),
			ModelDb.Card<Foraging>(),
			ModelDb.Card<FullBellyPunch>(),
			ModelDb.Card<TheWrathOfTheFoodGod>(),
			ModelDb.Card<ElixirMushroom>(),
			ModelDb.Card<Panacea>(),
		};
	}

	protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		List<CardModel> list = cards.ToList();

		// 原版游戏的卡牌通过“时代”（Epochs）系统来逐步解锁。
		// 如果您的 Mod 计划使用这种渐进式解锁机制，可以创建自定义的 Epoch 类并参考以下格式进行过滤。
		// 如果不需要分阶段解锁卡牌，直接返回 list 即可。
		/*
		if (!unlockState.IsEpochRevealed<Scout2Epoch>())
		{
			list.RemoveAll((CardModel c) => Scout2Epoch.Cards.Any((CardModel card) => card.Id == c.Id));
		}
		*/

		return list;
	}
}
