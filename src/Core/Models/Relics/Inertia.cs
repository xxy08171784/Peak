using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 惯性：如果本回合内打出 5 张以上的牌，则下回合额外抽 2 张牌。
/// 罕见稀有度。
/// </summary>
public sealed class Inertia : RelicModel
{
	private int _cardsPlayedThisTurn;
	private bool _bonusDrawNextTurn;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override Task BeforeCombatStart()
	{
		_cardsPlayedThisTurn = 0;
		_bonusDrawNextTurn = false;
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner?.Creature))
		{
			return;
		}

		// 如果上回合打够了 5 张牌，本回合额外抽 2 张
		if (_bonusDrawNextTurn)
		{
			_bonusDrawNextTurn = false;
			Flash();
			await CardPileCmd.Draw(
				new ThrowingPlayerChoiceContext(), 2m, base.Owner);
		}

		// 重置计数
		_cardsPlayedThisTurn = 0;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}

		_cardsPlayedThisTurn++;

		if (_cardsPlayedThisTurn > 5)
		{
			_bonusDrawNextTurn = true;
		}

		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		_cardsPlayedThisTurn = 0;
		_bonusDrawNextTurn = false;
		return Task.CompletedTask;
	}
}
