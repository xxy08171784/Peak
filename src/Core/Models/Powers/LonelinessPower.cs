using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 孤独：Boss 每受到一次伤害，在本回合失去 2 点力量（可叠加）。
/// </summary>
public sealed class LonelinessPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    /// <summary>
    /// 在 Boss 受到伤害后被触发一次。本回合 -2 力量。
    /// （由 LeaderMiles AI 在攻击动作中主动调用的辅助方法触发）
    /// </summary>
    public static async Task OnBossHit(Creature boss, PlayerChoiceContext ctx)
    {
        if (boss == null) return;
        // 每段伤害触发一次 -2力量
        await PowerCmd.Apply<StrengthPower>(ctx, boss, -2m, boss, null);
    }
}