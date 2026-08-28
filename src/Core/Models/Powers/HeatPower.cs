using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

public sealed class HeatPower : PowerModel
{
	// 炎热值是给自己施加的正向属性，因此是 Buff
	public override PowerType Type => PowerType.Debuff;
	
	// 使用层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 显式限制：不允许炎热值变为负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每当炎热值层数发生改变时触发。
	/// - 获得炎热值（amount &gt; 0）：若还没有散热，则补 1 层散热（首次从无到有获得时触发）。
	/// - 失去炎热值（amount &lt; 0）：对随机一名敌人造成等同于失去层数的伤害（纵火高手可使伤害翻倍）。
	/// </summary>
	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		// 只处理炎热值自身的变化
		if (power != this)
		{
			return;
		}

		if (amount > 0)
		{
			// 获得炎热值：若还没有散热，则补 1 层
			if (Owner.GetPower<RejectionOfHeatPower>() == null)
			{
				await PowerCmd.Apply<RejectionOfHeatPower>(choiceContext, Owner, 1, applier, cardSource);
			}

			// 过载判定：三 buff 总和 ≥ 100 时触发（炎热最多优先）
			await BuffOverloadChecker.TryTrigger(Owner, choiceContext);
			return;
		}

		if (amount >= 0)
		{
			return;
		}

		// —— 失去炎热值：对敌人造成等同于失去层数的伤害 ——
		int layersLost = (int)-amount; // amount 为负，取绝对值即失去的层数
		if (layersLost <= 0)
		{
			return;
		}

		Flash(); // 状态图标闪烁，提示玩家触发了效果

		// 获取当前所有可被击中的敌人
		List<Creature> aliveEnemies = Owner.CombatState.HittableEnemies.ToList();
		if (aliveEnemies.Count == 0)
		{
			return;
		}

		// 检查玩家是否有纵火高手 Buff：
		// 无 → 对随机一名敌人造成伤害；有 → 伤害翻倍，并对所有敌人造成伤害（AOE）
		ArsonExpertPower? arsonExpert = Owner.GetPower<ArsonExpertPower>();
		int finalDamage = arsonExpert != null ? layersLost * arsonExpert.DamageMultiplier : layersLost;

		if (arsonExpert != null)
		{
			// 纵火高手：AOE，所有敌人受同等伤害
			VfxCmd.PlayOnCreatureCenters(aliveEnemies, "vfx/vfx_attack_slash");
			await CreatureCmd.Damage(
				choiceContext,
				(IEnumerable<Creature>)aliveEnemies,
				(decimal)finalDamage,
				ValueProp.Unpowered,
				Owner
			);
		}
		else
		{
			// 无纵火高手：随机一名敌人
			Creature? target = Owner.Player?.RunState.Rng.CombatTargets.NextItem(aliveEnemies);
			if (target == null)
			{
				return;
			}
			VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
			await CreatureCmd.Damage(
				choiceContext,
				target,
				(decimal)finalDamage,
				ValueProp.Unpowered,
				Owner
			);
		}
	}
}
