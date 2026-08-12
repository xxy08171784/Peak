using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山杖：每次切换场景时获得 3 点防御。
/// 通过订阅 MyClimbing.EnvironmentChanged 静态事件来实时响应（包括 Climb 卡牌触发的切换）。
/// 商店稀有度。
/// </summary>
public sealed class Alpenstock : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	private void OnEnvironmentChanged(Player player, int previousValue, int newValue)
	{
		if (player != base.Owner || base.Owner?.Creature == null)
		{
			return;
		}

		Flash();
		TaskHelper.RunSafely(
			CreatureCmd.GainBlock(base.Owner.Creature, 3m, ValueProp.Move, null));
	}

	public override Task BeforeCombatStart()
	{
		MyClimbing.EnvironmentChanged += OnEnvironmentChanged;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		MyClimbing.EnvironmentChanged -= OnEnvironmentChanged;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
