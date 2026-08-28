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
        // 上一个获得的遗物 = 列表倒数第二个（最后一个就是本遗物自身）
        if (all.Count < 2) { GD.Print("[SH] No previous relic"); return; }
        var prev = all[all.Count - 2];
        if (prev.Rarity == RelicRarity.Starter || prev.Rarity == RelicRarity.Ancient || prev.Rarity == RelicRarity.None)
        {
            GD.Print("[SH] Previous relic is starter/ancient/gem, skip");
            return;
        }
        Flash();
        var canonical = ModelDb.AllRelics.FirstOrDefault(m => m.GetType() == prev.GetType());
        if (canonical == null) { GD.Print("[SH] No canonical"); return; }
        await RelicCmd.Obtain(canonical.ToMutable(), base.Owner);
    }
}