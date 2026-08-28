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
/// 每回合开始时获得1点格挡。
/// </summary>
public sealed class ScoutPerseverance : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_perseverance";

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (base.Owner?.Creature == null) return;
        if (!participants.Any(c => c == base.Owner.Creature)) return;
        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(1m, ValueProp.Unpowered), null);
    }
}