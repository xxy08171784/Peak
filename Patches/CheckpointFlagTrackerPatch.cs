using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using peak.Core.Models.Cards;

namespace peak.Patches;

/// <summary>
/// Harmony 补丁：在玩家回合结束时记录每位玩家的生命值，
/// 供【检查点旗帜】回溯使用。
/// 挂载在 CombatManager.EndPlayerTurnPhaseTwoInternal（无参重载）的 Postfix：
/// 该方法是玩家回合结束的核心清理流程（结算完所有玩家动作、弃牌后）。
/// 同时在战斗结束时清空记录，防止跨战斗残留。
/// </summary>
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.EndPlayerTurnPhaseTwoInternal), new Type[0])]
public static class CheckpointFlagTrackerPatch
{
	private static bool _subscribed;

	static void Postfix()
	{
		// 惰性订阅战斗结束事件，用于清空记录
		if (!_subscribed)
		{
			_subscribed = true;
			CombatManager.Instance.CombatEnded += _ => CheckpointFlagTracker.Clear();
		}

		CombatManager combatManager = CombatManager.Instance;
		if (combatManager == null)
		{
			return;
		}

		// CombatTurnState 是 internal 类型，无法直接引用，全程用反射读取
		var turnState = Traverse.Create(combatManager).Field("_turnState").GetValue();
		if (turnState == null)
		{
			return;
		}
		var state = Traverse.Create(turnState).Property("State").GetValue();
		if (state == null)
		{
			return;
		}

		// 读取 CombatState.Players (IReadOnlyList<Player>)
		var players = Traverse.Create(state).Property("Players").GetValue() as System.Collections.Generic.IReadOnlyList<Player>;
		if (players == null)
		{
			return;
		}

		foreach (Player player in players)
		{
			if (player?.Creature != null && !player.Creature.IsDead)
			{
				CheckpointFlagTracker.RecordTurnEndHp(player, player.Creature.CurrentHp);
			}
		}
	}
}
