using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 希望 — 记录玩家获得的希望层数。
/// 当层数达标时（1人5/2人9/3人13/4人18），Boss 直接死亡（奇迹发生）。
/// 层数由「祈愿」和「宽恕」卡牌增加。
/// </summary>
public sealed class HopePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    private bool _hasTriggered;

    /// <summary>
    /// 根据玩家人数获取希望门槛。
    /// </summary>
    public static int GetThreshold(int playerCount)
    {
        return playerCount switch
        {
            1 => 5,
            2 => 9,
            3 => 13,
            4 => 18,
            _ => 99
        };
    }

    /// <summary>
    /// 希望层数变化后检测是否达标。
    /// amount > 0 表示获得希望（判定时机），达标后 Boss 直接死亡。
    /// </summary>
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount <= 0m || _hasTriggered)
        {
            return;
        }

        int playerCount = Owner.CombatState.Players.Count(p => p.Creature?.IsAlive == true);
        if (Amount < GetThreshold(playerCount))
        {
            return;
        }

        _hasTriggered = true;
        Flash();

        // 好结局：给所有玩家「童军的荣耀」遗物，然后 Boss 直接死亡
        foreach (var player in Owner.CombatState.Players)
        {
            if (player.Creature?.IsAlive == true)
            {
                await RelicCmd.Obtain<ScoutGlory>(player);
            }
        }

        // Boss 直接死亡（奇迹发生）
        await CreatureCmd.Kill(Owner, force: true);
    }
}