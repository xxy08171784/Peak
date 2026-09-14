using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的毅力 - 第2颗宝石遗物。
/// 火堆「坚定」选项获得。
/// 每回合结束时获得1点格挡。
/// </summary>
public sealed class ScoutPerseverance : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.None;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_perseverance";

    /// <summary>
    /// 回合结束时获得格挡（参考原版 FakeOrichalcum 的写法）。
    /// 此时获得的格挡会保留到敌方回合，用于抵挡敌人的攻击。
    /// </summary>
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (base.Owner?.Creature == null) return;
        if (!participants.Any(c => c == base.Owner.Creature)) return;
        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(1m, ValueProp.Unpowered), null);
    }
}