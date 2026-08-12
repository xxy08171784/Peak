using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Relics;

/// <summary>
/// 火柴：在战斗开始时，在本场战斗中随机升级你抽牌堆中的一张食物牌。
/// 普通稀有度。
/// </summary>
public sealed class Match : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		// 获取抽牌堆中可以升级的食物卡
		List<CardModel> foodCards = PileType.Draw
			.GetPile(base.Owner)
			.Cards
			.Where(c => c is IFoodCard && c.IsUpgradable)
			.ToList();

		if (foodCards.Count == 0)
		{
			return;
		}

		// 随机选一张升级
		CardModel? target = foodCards
			.StableShuffle(base.Owner.RunState.Rng.CombatCardSelection)
			.FirstOrDefault();

		if (target != null)
		{
			Flash();
			CardCmd.Upgrade(target);
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
