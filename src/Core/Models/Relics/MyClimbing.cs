using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Cards;
using peak.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace peak.Core.Models.Relics;

public class MyClimbing : RelicModel
{
	/// <summary>
	/// 场景切换事件，参数为新场景值（0-3）。
	/// 其他遗物（如 Alpenstock）可订阅此事件来响应场景变化。
	/// </summary>
	public event Action<int>? SceneChanged;

	private int _environmentValue = 0;
	private bool _hasInitializedThisCombat = false;

	// 这是 Scout 的初始遗物，稀有度必须为 Starter，
	// 否则会进入遗物袋被宝箱/精英/事件再次开出（官方初始遗物均为 Starter，会被遗物袋自动过滤）
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override bool ShowCounter => CombatManager.Instance?.IsInProgress ?? false;

	public override int DisplayAmount => EnvironmentValue;

	protected int EnvironmentValue
	{
		get => _environmentValue;
		set
		{
			AssertMutable();
			_environmentValue = value;
			UpdateDisplay();
		}
	}

	protected void UpdateDisplay()
	{
		InvokeDisplayAmountChanged();
	}

	/// <summary>
	/// 修改环境值并触发一次效果
	/// </summary>
	public async Task ModifyEnvironmentValue(PlayerChoiceContext choiceContext, int amount)
	{
		int previousValue = EnvironmentValue;

		int newValue = (previousValue + amount) % 4;
		if (newValue < 0)
		{
			newValue += 4;
		}

		EnvironmentValue = newValue;

		// 通知订阅者场景已切换
		SceneChanged?.Invoke(newValue);

		// 触发阶段事件
		await TriggerEnvironmentEffect(choiceContext, previousValue, EnvironmentValue);
	}

	private async Task TriggerEnvironmentEffect(PlayerChoiceContext choiceContext, int previousValue, int currentValue)
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		Flash();

		// 执行当前状态的效果
		await ExecuteStateEffect(choiceContext, currentValue);
	}

	/// <summary>
	/// 公共方法：无视场景切换条件，直接强行触发指定场景的效果（供卡牌调用）
	/// </summary>
	public async Task ExecuteStateEffect(PlayerChoiceContext choiceContext, int stateValue)
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		switch (stateValue)
		{
			case 0:
				// 海岛：获得 3 层 覆甲
				await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
				break;

			case 1:
				// 森蕈：将一张带虚无的蘑菇盲盒加入手牌
				await GainRandomMushroom(choiceContext);
				break;

			case 2:
				// 方山：获得 15 层炎热
				await GainHeat(choiceContext, 15m);
				break;

			case 3:
				// 雪山：失去至多 10 层炎热，给予所有敌人一层寒冷
				await HandleTemperatureShift(choiceContext);
				break;
		}
	}

	#region 回合/战斗生命周期

	/// <summary>
	/// 战斗开始时：把计数切到 0（第一回合即处于海岛状态，触发 0 号效果）。
	/// </summary>
	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		_hasInitializedThisCombat = true;

		// 计数切到 0
		_environmentValue = 0;
		UpdateDisplay();
		SceneChanged?.Invoke(0);

		var choiceContext = new ThrowingPlayerChoiceContext();

		// 触发 0 号（海岛）效果
		await ExecuteStateEffect(choiceContext, 0);
	}

	/// <summary>
	/// 每回合开始时：计数 +1（3 +1 回到 0，0123 循环），并触发对应数字的效果。
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		// 只在自己的回合触发
		if (!participants.Contains(base.Owner.Creature))
		{
			return;
		}

		// 战斗首回合由 BeforeCombatStart 初始化（切到 0 并触发海岛），这里跳过以免重复
		if (!_hasInitializedThisCombat)
		{
			_hasInitializedThisCombat = true;

			// 兜底：如果 BeforeCombatStart 没有跑（理论上不会），首回合切到 0
			_environmentValue = 0;
			UpdateDisplay();
			SceneChanged?.Invoke(0);
			return;
		}

		if (base.Owner.PlayerCombatState?.TurnNumber <= 1)
		{
			// 第一回合：计数已在 BeforeCombatStart 设为 0 并触发过，直接跳过
			return;
		}

		// 第二回合起：计数 +1 并触发新场景效果
		int previousValue = EnvironmentValue;
		int newValue = (previousValue + 1) % 4;
		EnvironmentValue = newValue;
		SceneChanged?.Invoke(newValue);

		// 回合开始钩子无 PlayerChoiceContext，使用后台安全上下文
		var choiceContext = new ThrowingPlayerChoiceContext();

		Flash();
		await ExecuteStateEffect(choiceContext, newValue);
	}

	#endregion

	#region 核心效果实现

	/// <summary>
	/// 森蕈：将一张带虚无的蘑菇盲盒加入手牌。
	/// </summary>
	private async Task GainRandomMushroom(PlayerChoiceContext choiceContext)
	{
		if (base.Owner == null || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		// 创建蘑菇盲盒（本身自带虚无关键词），加入手牌
		MushroomBoxSet box = base.Owner.Creature.CombatState.CreateCard<MushroomBoxSet>(base.Owner);
		await CardPileCmd.AddGeneratedCardsToCombat(new[] { box }, PileType.Hand, base.Owner);
	}

	/// <summary>
	/// 雪山：失去至多 10 层炎热（不会降到负数），给予所有敌人 1 层寒冷。
	/// </summary>
	private async Task HandleTemperatureShift(PlayerChoiceContext choiceContext)
	{
		HeatPower? heatPower = base.Owner.Creature.GetPower<HeatPower>();
		if (heatPower != null && heatPower.Amount > 0)
		{
			// 计算实际能扣减的层数（最多 10 层，防止层数不足 10 时算成负数）
			int reduceAmount = Math.Min(heatPower.Amount, 10);
			await PowerCmd.ModifyAmount(choiceContext, heatPower, -reduceAmount, null, null);
		}

		IEnumerable<Creature> targets = base.Owner.Creature.CombatState.HittableEnemies;
		foreach (Creature enemy in targets)
		{
			await PowerCmd.Apply<ColdPower>(choiceContext, enemy, 1m, base.Owner.Creature, null);
		}
	}

	/// <summary>
	/// 熔炉：获得指定层数的炎热。
	/// </summary>
	private async Task GainHeat(PlayerChoiceContext choiceContext, decimal amount)
	{
		await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, null);
	}

	#endregion

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		_environmentValue = 0;
		_hasInitializedThisCombat = false;
		return Task.CompletedTask;
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}
