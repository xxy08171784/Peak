using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 惯性：如果在本回合内打出 5 张以上的牌，则下回合额外抽 2 张牌。
/// 罕见稀有度。
/// 参考 Pocketwatch 的 ModifyHandDraw 模式实现跨回合效果。
/// </summary>
public sealed class Inertia : RelicModel
{
	private int _cardsPlayedThisTurn;
	private int _cardsPlayedLastTurn;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner)
		{
			_cardsPlayedThisTurn++;
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// 在抽牌阶段修改抽牌数量：上回合打出 >5 张则 +2。
	/// </summary>
	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner)
		{
			return count;
		}
		// 第一回合无"上回合"
		if (base.Owner!.PlayerCombatState!.TurnNumber == 1)
		{
			return count;
		}
		if (_cardsPlayedLastTurn > 5)
		{
			return count + 2m;
		}
		return count;
	}

	public override Task AfterModifyingHandDraw()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(
		PlayerChoiceContext choiceContext,
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner!.Creature))
		{
			return Task.CompletedTask;
		}
		// 保存上回合数据，重置本回合计数
		_cardsPlayedLastTurn = _cardsPlayedThisTurn;
		_cardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		_cardsPlayedThisTurn = 0;
		_cardsPlayedLastTurn = 0;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
