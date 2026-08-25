using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 坚实后盾：每当你在回合中获得格挡时，拥有「被抛弃者」的玩家获得其一半格挡。
/// 参考官方 BeaconOfHopePower（希望灯塔）的机制，通过 AfterBlockGained hook 触发。
/// </summary>
public sealed class SolidBackingPower : PowerModel
{
    private bool _hasAlreadyGivenBlock;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 防止多个坚实后盾互相触发导致无限循环。
    /// </summary>
    private bool HasAlreadyGivenBlock
    {
        get => _hasAlreadyGivenBlock;
        set
        {
            AssertMutable();
            _hasAlreadyGivenBlock = value;
        }
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        // 只响应"自己"获得格挡，且必须是玩家回合
        if (amount < 1m || creature != Owner || CombatState.CurrentSide != Owner.Side || HasAlreadyGivenBlock)
        {
            return;
        }

        decimal amountToGive = amount * 0.5m;
        if (amountToGive < 1m)
        {
            return;
        }

        // 找到拥有「被抛弃者」的玩家
        var marked = CombatState.Players
            .Select(p => p.Creature)
            .FirstOrDefault(c => c != null && c.IsAlive && c.IsPlayer && c != Owner && c.GetPower<AbandonedMarkPower>()?.Amount > 0);

        if (marked == null)
        {
            return;
        }

        HasAlreadyGivenBlock = true;
        await CreatureCmd.GainBlock(marked, amountToGive, ValueProp.Unpowered, null);
        HasAlreadyGivenBlock = false;
    }
}
