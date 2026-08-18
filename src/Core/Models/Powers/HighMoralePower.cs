using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 士气高涨：每当玩家切换环境时，获得 1 点覆甲并抽 1 张牌。
/// 覆甲固定为 1，抽牌固定为 1。
/// </summary>
public sealed class HighMoralePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	// 每次切换环境获得的覆甲（随层数叠加，层数即 Amount）
	public int BlockPerTrigger => Amount;

	// 每次切换环境抽的牌数（随层数叠加，层数即 Amount）
	public int CardsPerTrigger => Amount;

	// 是否已订阅事件
	private bool _subscribed;

	/// <summary>
	/// Power 被施加时订阅环境切换事件。
	/// </summary>
	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		SubscribeIfNeeded();
		return Task.CompletedTask;
	}

	private void SubscribeIfNeeded()
	{
		if (!_subscribed)
		{
			_subscribed = true;
			MyClimbing.EnvironmentChanged += OnEnvironmentChanged;
		}
	}

	private async void OnEnvironmentChanged(Player player, int previous, int current)
	{
		// 只响应自己所属的玩家
		if (player != Owner.Player || Owner.IsDead || MegaCrit.Sts2.Core.Combat.CombatManager.Instance?.IsInProgress != true)
		{
			return;
		}

		Flash();

		var choiceContext = new ThrowingPlayerChoiceContext();

		// 1. 获得覆甲
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, BlockPerTrigger, Owner, null);

		// 2. 抽牌
		if (CardsPerTrigger > 0)
		{
			await CardPileCmd.Draw(choiceContext, CardsPerTrigger, player);
		}
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		MyClimbing.EnvironmentChanged -= OnEnvironmentChanged;
		_subscribed = false;
		return base.AfterCombatEnd(room);
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		MyClimbing.EnvironmentChanged -= OnEnvironmentChanged;
		_subscribed = false;
		return base.AfterRemoved(oldOwner);
	}
}
