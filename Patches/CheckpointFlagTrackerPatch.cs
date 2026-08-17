using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using peak.Core.Models.Cards;

namespace peak.Patches;

/// <summary>
/// Harmony 补丁：在每个玩家回合开始时记录该玩家当时的生命值，
/// 供【检查点旗帜】回溯使用。
/// 挂载在 Hook.AfterPlayerTurnStart 的 Postfix：
/// 该静态方法是玩家回合开始的核心流程（重置能量、抽牌完成后）的收尾钩子。
/// 同时在战斗结束时清空记录，防止跨战斗残留。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class CheckpointFlagTrackerPatch
{
	private static bool _subscribed;

	static void Postfix(Player player)
	{
		// 惰性订阅战斗结束事件，用于清空记录
		if (!_subscribed)
		{
			_subscribed = true;
			CombatManager.Instance.CombatEnded += _ => CheckpointFlagTracker.Clear();
		}

		if (player?.Creature != null && !player.Creature.IsDead)
		{
			CheckpointFlagTracker.RecordTurnStartHp(player, player.Creature.CurrentHp);
		}
	}
}
