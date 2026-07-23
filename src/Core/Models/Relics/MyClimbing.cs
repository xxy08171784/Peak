using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Powers; // 确保导入了你自己写的 HeatPower 和 ColdPower

namespace peak.Core.Models.Relics;

public sealed class MyClimbing : RelicModel
{
	private int _environmentValue = 0;

	public override RelicRarity Rarity => RelicRarity.Common;

	// 只在战斗进行中显示遗物计数器
	public override bool ShowCounter => CombatManager.Instance?.IsInProgress ?? false;

	// 遗物图标上显示的当前数字（0, 1, 2, 3）
	public override int DisplayAmount => EnvironmentValue;

	// 内部环境值属性，带有 StS2 规范的 AssertMutable 保护
	private int EnvironmentValue
	{
		get => _environmentValue;
		set
		{
			AssertMutable();
			_environmentValue = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		// 每次数值改变时，通知 UI 刷新显示
		InvokeDisplayAmountChanged();
	}

	/// <summary>
	/// 公共方法：供特定卡牌在打出时调用，用来改变环境值。
	/// </summary>
	/// <param name="choiceContext">出牌上下文</param>
	/// <param name="amount">改变的数值（可以是正数或负数）</param>
	public async Task ModifyEnvironmentValue(PlayerChoiceContext choiceContext, int amount)
	{
		int previousValue = EnvironmentValue;

		// 计算新值，并使用数学取模确保其循环在 0 ~ 3 之间
		int newValue = (previousValue + amount) % 4;
		if (newValue < 0)
		{
			newValue += 4; // 处理负数情况下的循环
		}

		// 更新内部状态
		EnvironmentValue = newValue;

		// 触发对应的阶段事件
		await TriggerEnvironmentEffect(choiceContext, previousValue, EnvironmentValue);
	}

	private async Task TriggerEnvironmentEffect(PlayerChoiceContext choiceContext, int previousValue, int currentValue)
	{
		// 安全拦截：遗物未装备到角色身上时不触发效果
		if (base.Owner?.Creature == null)
		{
			return;
		}

		// 闪烁一下遗物，提示玩家环境值发生了变化
		Flash();

		switch (currentValue)
		{
			case 0:
				// 只有在"到达 0"时触发（即上一次的值不是 0）
				if (previousValue != 0)
				{
					// 获得 3 层 覆甲 (PlatingPower)
					await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
				}
				break;

			case 1:
				// 获得一个随机蘑菇（暂时保留占位，不处理）
				await GainRandomMushroom(choiceContext);
				break;

			case 2:
				// 失去 10 炎热并给敌人 1 层寒冷
				await HandleTemperatureShift(choiceContext);
				break;

			case 3:
				// 获得 15 点炎热
				await GainHeat(choiceContext, 15m);
				break;
		}
	}

	#region 核心效果实现

	// 蘑菇效果：按要求，先保留占位
	private async Task GainRandomMushroom(PlayerChoiceContext choiceContext)
	{
		// TODO: 在此实现"获得随机蘑菇"的逻辑。
		await Task.CompletedTask;
	}

	/// <summary>
	/// 阶段 2：失去 10 点炎热，并给全体敌人施加 1 层寒冷。
	/// </summary>
	private async Task HandleTemperatureShift(PlayerChoiceContext choiceContext)
	{
		// 1. 玩家失去 10 点炎热
		await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, -10m, base.Owner.Creature, null);

		// 2. 获取所有可被击中的敌人并施加寒冷
		IEnumerable<Creature> targets = base.Owner.Creature.CombatState.HittableEnemies;

		foreach (Creature enemy in targets)
		{
			await PowerCmd.Apply<ColdPower>(choiceContext, enemy, 1m, base.Owner.Creature, null);
		}
	}

	/// <summary>
	/// 阶段 3：获得指定数值的炎热。
	/// </summary>
	private async Task GainHeat(PlayerChoiceContext choiceContext, decimal amount)
	{
		await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, null);
	}

	#endregion

	public override Task AfterCombatEnd(CombatRoom _)
	{
		// 战斗结束时，将状态和计数器重置为 0
		base.Status = RelicStatus.Normal;
		_environmentValue = 0;
		return Task.CompletedTask;
	}

	// 必须实现此抽象属性
	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();
}
