using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;
/// <summary>
/// insane：纯粹的标记 power（黑化宾邦复活时获得）。
/// 文案："这个角色在死亡时，将做出未知行为。"
/// 实际逻辑在 <see cref="peak.Core.Models.Monsters.Binbang"/> 的死亡/复活流程里：
/// 黑化形态死亡时不真正死亡，而是把血条变为 9999999 并切到「销毁」意图。
/// </summary>
public sealed class InsanePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}
