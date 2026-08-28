using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.RestSites;

/// <summary>
/// 火堆「坚定」选项 - 获得第2颗宝石遗物「童军的毅力」。
/// 选择后该玩家的所有火堆不再出现该选项。
/// </summary>
public sealed class FirmRestSiteOption : RestSiteOption
{
    public override string OptionId => "FIRM";

    private bool _alreadyUsed;

    public FirmRestSiteOption(Player owner) : base(owner) { }

    public override bool IsEnabled
    {
        get
        {
            if (_alreadyUsed) return false;
            if (Owner.Relics.Any(r => r is ScoutPerseverance)) return false;
            foreach (var p in Owner.RunState.Players)
                if (p.Relics.Any(r => r is ScoutPerseverance)) return false;
            return true;
        }
    }

    public override async Task<bool> OnSelect()
    {
        if (!IsEnabled) return false;
        var relic = ModelDb.Relic<ScoutPerseverance>().ToMutable();
        await RelicCmd.Obtain(relic, Owner);
        _alreadyUsed = true;
        return true;
    }
}