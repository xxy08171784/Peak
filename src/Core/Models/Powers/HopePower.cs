using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 希望 — 记录玩家获得的希望层数。
/// 当层数达标时（1人5/2人9/3人14/4人19），Boss 直接死亡（奇迹发生）。
/// 层数由「祈愿」和「宽恕」卡牌增加。
/// </summary>
public sealed class HopePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    /// <summary>
    /// 供本地化文本 <c>{Threshold}</c> 使用的显示变量。
    ///
    /// 引擎只会往 power 的 smartDescription 里注入固定几个变量（Amount / PlayerCount / OwnerName …）
    /// 以及本 power 的 CanonicalVars；文本里出现没人提供的变量名时 SmartFormat 会直接抛异常，
    /// LocManager 捕获后返回原始模板，于是玩家看到的是带 <c>{Threshold}</c> 花括号的未替换文本。
    ///
    /// 阈值取决于「当前存活玩家数」，中途有人阵亡就会从 19 掉到 14，所以不能做成固定值；
    /// 这里照原版 <see cref="StringVar"/> 的写法覆写 ToString，每次生成 Tooltip 时实时求值。
    /// 取值必须走 <c>_owner</c>（由 DynamicVarSet.InitializeWithOwner 赋值）而不是构造时捕获 this，
    /// 否则 power 被克隆成可变实例后，变量仍指向冻结的模板实例。
    /// </summary>
    private sealed class ThresholdVar : DynamicVar
    {
        public ThresholdVar()
            : base("Threshold", 0m)
        {
        }

        public override string ToString()
        {
            return (_owner as HopePower)?.Threshold.ToString() ?? "?";
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new ThresholdVar()
    };

    private bool _hasTriggered;

    /// <summary>
    /// 当前配置下的希望阈值（根据存活玩家数自动计算）。
    /// </summary>
    public int Threshold
    {
        get
        {
            var players = Owner?.CombatState?.Players;
            if (players == null) return 99;
            return GetThreshold(players.Count(p => p.Creature?.IsAlive == true));
        }
    }

    /// <summary>
    /// 根据玩家人数获取希望门槛。
    /// </summary>
    public static int GetThreshold(int playerCount)
    {
        return playerCount switch
        {
            1 => 5,
            2 => 9,
            3 => 14,
            4 => 19,
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

        // 好结局：给所有玩家「童军的荣耀」遗物，然后 Boss 直接死亡。
        // 不筛存活：好结局奖励与隐藏 Boss 入场券应人人都有，死亡玩家也不例外。
        foreach (var player in Owner.CombatState.Players)
        {
            await RelicCmd.Obtain<ScoutGlory>(player);
        }

        // Boss 直接死亡（奇迹发生）
        await CreatureCmd.Kill(Owner, force: true);
    }
}