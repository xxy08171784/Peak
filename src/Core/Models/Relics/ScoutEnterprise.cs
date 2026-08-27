using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的进取 - 第4颗宝石遗物。
/// 在打败第二个boss后获得。
/// 战斗开始时获得2点敏捷。
/// </summary>
public sealed class ScoutEnterprise : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.None;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_enterprise";

    public override async Task BeforeCombatStart()
    {
        if (base.Owner?.Creature == null) return;
        Flash();
        await PowerCmd.Apply<DexterityPower>(
            new ThrowingPlayerChoiceContext(),
            base.Owner.Creature, 2m,
            base.Owner.Creature, null);
    }
}