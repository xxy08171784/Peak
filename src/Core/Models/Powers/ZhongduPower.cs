using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 玩家版中毒（zhongdu）。
/// 效果与原版 PoisonPower 相同，但改为在【回合结束时】对玩家造成伤害。
/// 怪物身上的中毒不受影响，仍使用原版 PoisonPower。
/// 图标使用 zhongdu_power.png（由 Godot 按类名自动加载）。
/// </summary>
public sealed class ZhongduPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public override bool AllowNegative => false;

	/// <summary>计算触发次数（与原版逻辑一致）</summary>
	private int TriggerCount
	{
		get
		{
			int extra = Owner.CombatState?.GetOpponentsOf(Owner)?.Sum(c => c?.GetPowerAmount<AccelerantPower>() ?? 0) ?? 0;
			return Math.Min((int)Amount, 1 + extra);
		}
	}

	/// <summary>
	/// 获得中毒时触发过载判定：三 buff 总和 ≥ 100 时判定（中毒最多则获得 虚弱/易伤/脆弱 各1并失去20中毒）。
	/// 失去中毒（amount &lt; 0）不触发，避免死循环。
	/// </summary>
	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		if (power != this)
		{
			return;
		}
		if (amount > 0)
		{
			await BuffOverloadChecker.TryTrigger(Owner, choiceContext);
		}
	}

	/// <summary>回合开始时不做任何事（跳过原版中毒的回合开始触发）</summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		await Task.CompletedTask;
	}

	/// <summary>
	/// 被移除时触发（如骸骨之书用 PowerCmd.Remove 移除 debuff）。
	/// 若玩家有百毒不侵，则按移除的层数给予所有敌人原版中毒。
	/// </summary>
	public override async Task AfterRemoved(Creature oldOwner)
	{
		// 只在玩家身上且拥有百毒不侵时触发反击
		if (oldOwner.IsPlayer && oldOwner.GetPower<ImmuneToAllPoisonsPower>() != null)
		{
			int removedLayers = (int)Amount; // RemoveInternal 不清空 Amount，此处仍保留原层数
			if (removedLayers > 0)
			{
				ImmuneToAllPoisonsPower? immune = oldOwner.GetPower<ImmuneToAllPoisonsPower>();
				if (immune != null)
				{
					await immune.ApplyPoisonToAllEnemies(new ThrowingPlayerChoiceContext(), removedLayers * immune.PoisonPerLoss);
				}
			}
		}
		await Task.CompletedTask;
	}

	/// <summary>回合结束时：对玩家造成中毒伤害</summary>
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side != Owner.Side)
			return;

		// 百毒不侵：中毒不再对玩家造成伤害（但仍正常失去中毒层数）
		if (Owner.GetPower<ImmuneToAllPoisonsPower>() != null)
		{
			// 仍然扣除与触发次数等量的中毒层数（每失去1层会通过百毒不侵给予敌人中毒）
			int count = TriggerCount;
			for (int i = 0; i < count; i++)
			{
				if (Owner.IsAlive)
					await PowerCmd.Decrement(this);
				else
					await Cmd.CustomScaledWait(0.1f, 0.25f);
			}
			return;
		}

		int damageCount = TriggerCount;
		for (int i = 0; i < damageCount; i++)
		{
			await CreatureCmd.Damage(
				choiceContext, 
				Owner, Amount, 
				ValueProp.Unpowered, 
				null, 
				null);
			if (Owner.IsAlive)
				await PowerCmd.Decrement(this);
			else
				await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
	}
}