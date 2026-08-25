using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的荣耀 — 好结局奖励遗物。
/// 获得方式：在第4幕Boss战中达成希望层数达标后获得。
/// 效果：隐藏Boss入场券（后续实现隐藏Boss入口检测）。
/// </summary>
public sealed class ScoutGlory : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_glory";
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        // 暂为空—后续接入隐藏Boss入口逻辑
        await Task.CompletedTask;
    }
}