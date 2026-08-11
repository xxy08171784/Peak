using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace peak.Core.Models.Cards;

/// <summary>
/// 检查点追踪器：记录每位玩家"上回合结束时"的生命值。
/// 由 Harmony Patch（CombatManager.EndPlayerTurnPhaseTwoInternal 的 Postfix）
/// 在每个玩家回合结束时刷新记录。
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

	/// <summary>获取指定玩家上回合结束时的生命值。</summary>
	/// <returns>有记录返回记录值；无记录返回 null（用于卡面显示"回到多少血"）。</returns>
	public static int? GetLastTurnEndHp(Player? player)
	{
		if (player == null)
		{
			return null;
		}
		return _lastTurnEndHp.TryGetValue(player, out int hp) ? hp : null;
	}

	/// <summary>清空所有记录（战斗结束时调用，防止跨战斗残留）。</summary>
	public static void Clear()
	{
		_lastTurnEndHp.Clear();
	}
}
