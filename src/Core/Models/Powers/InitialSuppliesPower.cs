using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Cards;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 初始物资：每当环境切换到 0 海岛时，获得一张棉花糖。
/// </summary>
public sealed class InitialSuppliesPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

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
		// 只响应自己所属的玩家；且必须切换到 0 海岛
		if (player != Owner.Player || Owner.IsDead || current != 0)
		{
			return;
		}

		if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance?.IsInProgress != true)
		{
			return;
		}

		Flash();

		var choiceContext = new ThrowingPlayerChoiceContext();

		// 获得一张棉花糖（保留、消耗，获得 1(2) 点能量）
		Marshmallow marshmallow = Owner.CombatState.CreateCard<Marshmallow>(player);
		await CardPileCmd.AddGeneratedCardsToCombat(new[] { marshmallow }, PileType.Hand, player);
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
