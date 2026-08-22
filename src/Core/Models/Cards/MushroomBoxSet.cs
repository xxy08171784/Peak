using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 蘑菇盲盒：获得 5 种（升级后的）蘑菇中的随机一种。
/// 0 费，技能牌，普通稀有度，目标自身，虚无。
/// </summary>
public sealed class MushroomBoxSet : CardModel
{
	// 卡面图片（文件名与卡牌 ID 不一致，需显式指定）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/mushroom_box_set.png");

	

	// 虚无关键词（回合结束时自动消耗）
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

	// 悬停提示：预览 5 种蘑菇（主卡升级时，预览也动态显示升级版）
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromCard<Mushroom1>(base.IsUpgraded),
		HoverTipFactory.FromCard<Mushroom2>(base.IsUpgraded),
		HoverTipFactory.FromCard<Mushroom3>(base.IsUpgraded),
		HoverTipFactory.FromCard<Mushroom4>(base.IsUpgraded),
		HoverTipFactory.FromCard<Mushroom5>(base.IsUpgraded),
		HoverTipFactory.FromPower<SporePower>()
	};

	// 动态变量：新增效果——获得 2 层孢子
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SporePower>(2m)
	};

	public MushroomBoxSet()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 5 种蘑菇中随机选 1 种（等概率）
		bool isUpgraded = base.IsUpgraded;
		int roll = base.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 5);
		Godot.GD.Print($"[MushroomBoxSet] roll={roll}, isUpgraded={isUpgraded}");

		switch (roll)
		{
			case 0:
				await Mushroom1.CreateInHand(base.Owner, 1, isUpgraded, base.CombatState);
				break;
			case 1:
				await Mushroom2.CreateInHand(base.Owner, 1, isUpgraded, base.CombatState);
				break;
			case 2:
				await Mushroom3.CreateInHand(base.Owner, 1, isUpgraded, base.CombatState);
				break;
			case 3:
				await Mushroom4.CreateInHand(base.Owner, 1, isUpgraded, base.CombatState);
				break;
			case 4:
				await Mushroom5.CreateInHand(base.Owner, 1, isUpgraded, base.CombatState);
				break;
		}

			// 新增效果（不影响原随机蘑菇效果）：获得 2 层孢子
			await PowerCmd.Apply<SporePower>(
				choiceContext,
				base.Owner.Creature,
				base.DynamicVars["SporePower"].BaseValue,
				base.Owner.Creature,
				this
			);
		}

	protected override void OnUpgrade()
	{
		// 升级后：生成的蘑菇自动升级（无其他数值变化）
	}
}
