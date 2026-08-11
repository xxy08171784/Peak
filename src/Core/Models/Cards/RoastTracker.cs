using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 烤牌状态追踪器。
/// 记录"本场战斗中被烤过的卡牌实例"（仅按引用追踪，不持有强引用，防止内存泄漏）。
/// 用于【大炮】等卡牌判断"是否被烤过"：被烤过的卡牌打出时触发强化效果。
/// </summary>
public static class RoastTracker
{
	private static readonly ConditionalWeakTable<CardModel, object> _roastedCards = new();
	// ConditionalWeakTable 的 value 不能为 null，用哨兵对象占位
	private static readonly object _marker = new();

	/// <summary>标记一张卡牌已被烤。</summary>
	public static void MarkRoasted(CardModel card)
	{
		_roastedCards.Remove(card);
		_roastedCards.Add(card, _marker);
	}

	/// <summary>判断一张卡牌是否被烤过。</summary>
	public static bool WasRoasted(CardModel card)
	{
		return _roastedCards.TryGetValue(card, out _);
	}
}
