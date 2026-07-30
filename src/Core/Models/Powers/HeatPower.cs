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
    public override PowerType Type => PowerType.Buff;
	
    // 使用层数堆叠
    public override PowerStackType StackType => PowerStackType.Counter;

    // 显式限制：不允许炎热值变为负数
    public override bool AllowNegative => false;

	/// <summary>
	/// 回合即将结束前结算：扣减最多 7 层炎热值，并对所有活着的敌人造成等同于减少层数的伤害。
	/// </summary>
	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		// 1. 判定是否为宿主的回合结束
		if (side != Owner.Side)
		{
			return;
		}

        // 如果当前层数已经小于等于 0，则直接不触发
        if (Amount <= 0)
        {
            return;
        }

        // 2. 计算实际需要扣减的层数（至多 7 层，且绝不为负数）
        int layersToDecrease = Math.Clamp(Amount, 0, 7);
        if (layersToDecrease <= 0)
        {
            return;
        }

		Flash(); // 状态图标闪烁，提示玩家触发了效果

		// 3. 扣减炎热值层数（传入负数）
		await PowerCmd.ModifyAmount(choiceContext, this, -layersToDecrease, null, null);

        // 4. 获取当前所有可被击中的敌人
        List<Creature> aliveEnemies = Owner.CombatState.HittableEnemies.ToList();

        if (aliveEnemies.Count > 0)
        {
            // 5. 视觉效果：在每一个敌人身上播放受击斩击特效
            VfxCmd.PlayOnCreatureCenters(aliveEnemies, "vfx/vfx_attack_slash");

            // 6. 调用底层伤害指令
            await CreatureCmd.Damage(
                choiceContext, 
                (IEnumerable<Creature>)aliveEnemies, // 1. 显式转为 IEnumerable
                (decimal)layersToDecrease,           // 2. 显式转为 decimal
                ValueProp.Unpowered, 
                Owner
            );
        }
    }
}
