using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山杖：每次切换场景时获得 3 点防御。
/// 通过 IEnvironmentAware 接口（替代旧的 MyClimbing.EnvironmentChanged 静态事件）。
/// 商店稀有度。
/// </summary>
public sealed class Alpenstock : RelicModel, IEnvironmentAware
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public Task OnEnvironmentChanged(PlayerChoiceContext choiceContext, Player player, int previousValue, int newValue)
	{
		if (player != base.Owner || base.Owner?.Creature == null)
		{
			return Task.CompletedTask;
		}

		Flash();
		return CreatureCmd.GainBlock(base.Owner.Creature, 3m, ValueProp.Move, null);
	}
}
