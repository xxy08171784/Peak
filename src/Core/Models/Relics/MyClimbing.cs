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
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
<<<<<<< HEAD
using peak.Core.Models.Powers; // 确保导入了你自己写的 HeatPower 和 ColdPower
=======
using peak.Core.Models.Powers; 
>>>>>>> origin/xxy

namespace peak.Core.Models.Relics;

public sealed class MyClimbing : RelicModel
{
	private int _environmentValue = 0;

	public override RelicRarity Rarity => RelicRarity.Common;

<<<<<<< HEAD
	// 只在战斗进行中显示遗物计数器
=======
>>>>>>> origin/xxy
	public override bool ShowCounter => CombatManager.Instance?.IsInProgress ?? false;

	public override int DisplayAmount => EnvironmentValue;

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
		InvokeDisplayAmountChanged();
	}

	/// <summary>
	/// 修改环境值并触发一次效果
	/// </summary>
	public async Task ModifyEnvironmentValue(PlayerChoiceContext choiceContext, int amount)
	{
		int previousValue = EnvironmentValue;

<<<<<<< HEAD
		// 计算新值，并使用数学取模确保其循环在 0 ~ 3 之间
=======
>>>>>>> origin/xxy
		int newValue = (previousValue + amount) % 4;
		if (newValue < 0)
		{
			newValue += 4; 
		}

		EnvironmentValue = newValue;

		// 触发阶段事件
		await TriggerEnvironmentEffect(choiceContext, previousValue, EnvironmentValue);
	}

	private async Task TriggerEnvironmentEffect(PlayerChoiceContext choiceContext, int previousValue, int currentValue)
	{
<<<<<<< HEAD
		// 安全拦截：遗物未装备到角色身上时不触发效果
=======
>>>>>>> origin/xxy
		if (base.Owner?.Creature == null)
		{
			return;
		}

<<<<<<< HEAD
		// 闪烁一下遗物，提示玩家环境值发生了变化
=======
>>>>>>> origin/xxy
		Flash();

		// 如果切换后和切换前都是 0，说明没有发生实质切换，拦截防止重复触发
		if (currentValue == 0 && previousValue == 0)
		{
			return;
		}

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
<<<<<<< HEAD
				// 只有在"到达 0"时触发（即上一次的值不是 0）
				if (previousValue != 0)
				{
					// 获得 3 层 覆甲 (PlatingPower)
					await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
				}
=======
				// 获得 3 层 覆甲
				await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, null);
>>>>>>> origin/xxy
				break;

			case 1:
				await GainRandomMushroom(choiceContext);
				break;

			case 2:
				await HandleTemperatureShift(choiceContext);
				break;

			case 3:
				await GainHeat(choiceContext, 15m);
				break;
		}
	}

	#region 核心效果实现

	private async Task GainRandomMushroom(PlayerChoiceContext choiceContext)
	{
<<<<<<< HEAD
		// TODO: 在此实现"获得随机蘑菇"的逻辑。
		await Task.CompletedTask;
	}

	/// <summary>
	/// 阶段 2：失去 10 点炎热，并给全体敌人施加 1 层寒冷。
	/// </summary>
=======
		await Task.CompletedTask;
	}

>>>>>>> origin/xxy
	private async Task HandleTemperatureShift(PlayerChoiceContext choiceContext)
	{
		await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, -10m, base.Owner.Creature, null);

<<<<<<< HEAD
		// 2. 获取所有可被击中的敌人并施加寒冷
		IEnumerable<Creature> targets = base.Owner.Creature.CombatState.HittableEnemies;

=======
		IEnumerable<Creature> targets = base.Owner.Creature.CombatState.HittableEnemies;
>>>>>>> origin/xxy
		foreach (Creature enemy in targets)
		{
			await PowerCmd.Apply<ColdPower>(choiceContext, enemy, 1m, base.Owner.Creature, null);
		}
	}

<<<<<<< HEAD
	/// <summary>
	/// 阶段 3：获得指定数值的炎热。
	/// </summary>
=======
>>>>>>> origin/xxy
	private async Task GainHeat(PlayerChoiceContext choiceContext, decimal amount)
	{
		await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, null);
	}

	#endregion

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		_environmentValue = 0;
		return Task.CompletedTask;
	}

<<<<<<< HEAD
	// 必须实现此抽象属性
	protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();
}
=======
	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();
}
>>>>>>> origin/xxy
