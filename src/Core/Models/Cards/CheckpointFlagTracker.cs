using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace peak.Core.Models.Cards;

/// <summary>
/// 检查点追踪器：记录每位玩家"上回合结束时"的生命值。
/// 由【检查点追踪器 Power】在每个玩家回合结束时更新。
/// </summary>
public static class CheckpointFlagTracker
{
	// 玩家 -> 上回合结束时的生命值
	private static readonly Dictionary<Player, int> _lastTurnEndHp = new();

	/// <summary>记录指定玩家上回合结束时的生命值。</summary>
	public static void RecordTurnEndHp(Player player, int hp)
	{
		_lastTurnEndHp[player] = hp;
	}

	/// <summary>获取指定玩家上回合结束时的生命值（无记录时返回当前生命值）。</summary>
	public static int GetLastTurnEndHp(Player player)
	{
		return _lastTurnEndHp.TryGetValue(player, out int hp) ? hp : player.Creature.CurrentHp;
	}
}
