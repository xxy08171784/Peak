using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 滚雪球：打出后：
/// - 如果没有雪球，生成 1 张雪球加入手牌；
/// - 如果手牌中有雪球，升级雪球；
/// - 如果雪球存在但不在手牌，将雪球放入手牌并升级。
/// 打出后滚雪球本身返回手牌。
/// 1 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class SnowballRolling : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/snowball_rolling.png");

	public SnowballRolling()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 搜索已存在的雪球
		Snowball? existingSnowball = base.Owner.PlayerCombatState.AllCards
			.OfType<Snowball>()
			.FirstOrDefault();

		if (existingSnowball == null)
		{
			// 1a. 没有雪球 -> 生成一张加入手牌（从未升级状态开始）
			Snowball snowball = base.CombatState.CreateCard<Snowball>(base.Owner);
			await CardPileCmd.AddGeneratedCardsToCombat(new[] { snowball }, PileType.Hand, base.Owner);
		}
		else
		{
			bool wasInHand = existingSnowball.Pile?.Type == PileType.Hand;

			// 1b. 雪球不在手牌 -> 先移回手牌
			if (!wasInHand)
			{
				await CardPileCmd.Add(existingSnowball, PileType.Hand);
			}

			// 1c. 升级雪球：
			//     - 雪球本就在手牌中：连续堆雪球，升级 +1（0→1→2→3...）
			//     - 雪球被召回（不在手牌）：从 0 级重新开始，先降级再升到 1 级
			if (!wasInHand && existingSnowball.CurrentUpgradeLevel > 0)
			{
				existingSnowball.DowngradeInternal();
			}
			existingSnowball.UpgradeInternal();
			existingSnowball.FinalizeUpgradeInternal();
		}

		// 2. 打出后，滚雪球本身返回手牌底部
		await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom);
	}

	protected override void OnUpgrade()
	{
		// 升级后依旧 1 费，新增保留词条
		AddKeyword(CardKeyword.Retain);
	}
}
