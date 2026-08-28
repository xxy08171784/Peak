using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace peak.Core.Models.Relics;

/// <summary>
/// 环境切换感知接口。
/// 替代旧的 MyClimbing.EnvironmentChanged 静态事件 + async void 模式，
/// 由 MyClimbing 遍历 Power/Relic 列表直接调用，确保全部在 Task 链中等候完成，
/// 消除联机锁步同步中的 async void fire-and-forget 分叉风险。
/// </summary>
public interface IEnvironmentAware
{
    /// <summary>
    /// 当 MyClimbing 遗物的环境值切换时调用。
    /// </summary>
    /// <param name="choiceContext">调用者传入的上下文</param>
    /// <param name="player">所属玩家</param>
    /// <param name="previousValue">切换前的环境值</param>
    /// <param name="currentValue">切换后的环境值</param>
    Task OnEnvironmentChanged(PlayerChoiceContext choiceContext, Player player, int previousValue, int currentValue);
}