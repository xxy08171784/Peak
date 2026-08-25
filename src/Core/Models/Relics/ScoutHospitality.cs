using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的盛情 - 第1颗宝石遗物。
/// 商店额外售卖（不占用正常遗物生成位），售价200金。
/// 选择一个已拥有的遗物，生成复制品（不能复制初始遗物和远古遗物）。
/// </summary>
public sealed class ScoutHospitality : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Shop;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_hospitality";
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (base.Owner == null) return;
        var all = base.Owner.Relics;
        var available = all
            .Where(r => r != this
                && r.Rarity != RelicRarity.Starter
                && r.Rarity != RelicRarity.Ancient
                && !(r is ScoutHospitality)
                && !(r is ScoutPerseverance)
                && !(r is ScoutAmbition)
                && !(r is ScoutEnterprise))
            .ToList();
        if (available.Count == 0) { GD.Print("[SH] None"); return; }
        var rng = new Rng(base.Owner.RunState.Rng.Seed);
        var sel = available[rng.NextInt(available.Count)];
        Flash();
        var canonical = (RelicModel)ModelDb.All.FirstOrDefault(m => m.GetType() == sel.GetType());
        if (canonical == null) { GD.Print("[SH] No canonical"); return; }
        await RelicCmd.Obtain(canonical.ToMutable(), base.Owner);
    }
}