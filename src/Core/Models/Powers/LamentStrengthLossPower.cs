using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 悲鸣带来的临时力量下降 —— 本回合 -3 力量，回合结束时恢复。
/// </summary>
public sealed class LamentStrengthLossPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Lament>();
    protected override bool IsPositive => false;
}

/// <summary>
/// 孤独效果带来的临时力量下降 —— Boss 每次受到玩家攻击时给予 -2 力量，回合结束时恢复。
/// </summary>
public sealed class LonelinessHitStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Lament>();
    protected override bool IsPositive => false;
}