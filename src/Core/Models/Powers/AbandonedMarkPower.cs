using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 被抛弃者：在玩家的回合结束时，失去 5 × 层数 点生命值。
/// 注意：此 Power 不能被任何清除效果移除（由 AbandonedMarkProtectionPatch 保护）。
/// </summary>
public sealed class AbandonedMarkPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    /// <summary>
    /// 回合结束时：失去 5 × 层数 点生命值（不可被格挡）。
    /// </summary>
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, System.Collections.Generic.IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        Flash();
        // 直接伤害，不可被格挡（使用 Unblockable 标记）
        await CreatureCmd.Damage(ctx, Owner, Amount * 5m, ValueProp.Unblockable | ValueProp.Move, null, null, null);
    }
}