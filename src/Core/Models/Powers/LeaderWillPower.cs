using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 领队意志 — Boss 状态标记 Power。
/// 当 Boss HP < 200 时触发转换：
/// - 本回合免疫伤害
/// - 清除所有玩家的「被抛弃者」
/// - 全员 3 层易伤
/// - 塞「悲鸣」+「祈愿」到抽牌堆顶
/// - 给 Boss 添加「希望」Power
/// - 切换到 Phase 2 意图
/// </summary>
public sealed class LeaderWillPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single; // 只能有一层
    public override bool AllowNegative => false;
}