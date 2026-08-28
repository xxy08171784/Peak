using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace peak.Core.Models.Cards;

/// <summary>
/// 检查点追踪器：记录每位玩家"玩家回合开始时"的生命值。
/// 由 Harmony Patch（Hook.AfterPlayerTurnStart 的 Postfix）在每个玩家回合开始时调用 RecordTurnStartHp。
/// 内部维护两个槽位：
///   - 当前回合开始血量（_currentTurnStartHp）
///   - 上回合开始血量（_lastTurnStartHp，即上一回合记录下的值）
/// 打出【检查点旗帜】时读取的是 _lastTurnStartHp（上回合开始血量）。
/// </summary>
public static class CheckpointFlagTracker
{
	// 玩家 -> 当前回合开始时的生命值
	private static readonly Dictionary<Player, int> _currentTurnStartHp = new();

	// 玩家 -> 上回合开始时的生命值
	private static readonly Dictionary<Player, int> _lastTurnStartHp = new();

	/// <summary>
	/// 记录指定玩家回合开始时的生命值。
	/// 调用时机：玩家回合开始。先把当前（旧）值下放到"上回合"槽位，再写入新值。
	/// </summary>
	public static void RecordTurnStartHp(Player player, int hp)
	{
		if (_currentTurnStartHp.TryGetValue(player, out int previousHp))
		{
			_lastTurnStartHp[player] = previousHp;
		}
		_currentTurnStartHp[player] = hp;
	}

	/// <summary>获取指定玩家"上回合玩家回合开始"时的生命值。</summary>
	/// <returns>有记录返回记录值；无记录返回 null（用于卡面显示"回到多少血"）。</returns>
	public static int? GetLastTurnStartHp(Player? player)
	{
		if (player == null)
		{
			return null;
		}
		return _lastTurnStartHp.TryGetValue(player, out int hp) ? hp : null;
	}

	/// <summary>清空所有记录（战斗结束时调用，防止跨战斗残留）。</summary>
	public static void Clear()
	{
		_currentTurnStartHp.Clear();
		_lastTurnStartHp.Clear();
	}
}
