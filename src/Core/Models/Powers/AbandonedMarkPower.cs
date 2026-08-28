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
/// 语义上「不可被移除」= 不能被「失去所有负面效果」类效果（骸骨之书、潘多拉餐盒等）移除，
/// 通过 AfterRemoved 重施加实现：层数 &gt; 0 时被强制移除 → 立即重新施加。
/// 层数归零的自然移除（坚定信念/相予砥砺/阶段转换 ModifyAmount 减到 0）不重施加。
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
        await CreatureCmd.Damage(ctx, Owner, Amount * 5m, ValueProp.Unblockable | ValueProp.Move, null, null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        // 仍有层数却被移除（"失去所有负面效果"类效果）→ 立即重新施加，保持不可被移除。
        // 注意：不能用 Harmony prefix 拦截 PowerCmd.Remove（async 方法被跳过时返回的 Task
        // 永不完成，会让骸骨之书/潘多拉餐盒的 await 永久挂起）。
        // 层数已归零的自然移除（坚定信念等）Amount == 0，不会走到重施加分支。
        if (Amount > 0 && oldOwner != null && oldOwner.CombatState != null
            && !CombatManager.Instance.IsEnding && oldOwner.CanReceivePowers)
        {
            await PowerCmd.Apply<AbandonedMarkPower>(
                new ThrowingPlayerChoiceContext(), oldOwner, Amount, Applier, null);
        }
        await base.AfterRemoved(oldOwner);
    }
}