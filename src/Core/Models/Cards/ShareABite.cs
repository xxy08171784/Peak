using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;using MegaCrit.Sts2.Core.Helpers;using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 分你一口：将手牌中的一张食物卡的复制品添加至一位其他玩家的手牌中。
/// 1 费（升级后 0 费），技能牌，罕见稀有度，目标任意队友。
/// 多人专属卡牌。
/// </summary>
public sealed class ShareABite : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 卡面图片（文件名与卡牌 ID 同名，统一小写：share_a_bite.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/share_a_bite.png");

	public ShareABite()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 从手牌中选择一张食物卡
		CardModel? food = (await CardSelectCmd.FromHand(
			context: choiceContext,
			player: base.Owner,
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
			filter: (CardModel c) => c is IFoodCard,
			source: this)).FirstOrDefault();

		if (food == null)
		{
			return; // 没有食物卡可选（可选跳过）
		}

		// 2. 复制该卡给目标玩家。必须用 CreateCloneForPlayer 转移所属玩家：
		//    CreateClone() 造出的克隆会保留原主，直接改 Owner 只是改字段，绕过了官方的所有权转移。
		CardModel clone = food.CreateCloneForPlayer(cardPlay.Target.Player!);
		await CardPileCmd.AddGeneratedCardsToCombat(
			new[] { clone },
			PileType.Hand,
			base.Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
