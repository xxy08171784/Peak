using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace peak.Patches;

/// <summary>
/// 第四层地图永远重新生成，不沿用存档地图。
///
/// 原因：第四层用的是自定义 <see cref="peak.Core.Map.NadirActMap"/>，其中"第二个 Boss（隐藏 Boss）"
/// 节点是**动态**的——只有队伍拿到「童军的荣耀」后才返回。而读档时 <c>RunManager.GenerateMap</c>
/// 会优先用 <c>SavedActMap</c>（保存时的地图快照），快照里没有那个动态节点，
/// 于是"读档后再打出好结局"将无法出现隐藏 Boss。
///
/// 这里在每次生成地图前丢弃 Act4（index 3）的存档地图，让它走
/// <c>State.Act.CreateMap</c> → BaseLib CustomCreateMap → NadirActMap 重新生成。
/// 走廊结构确定、且不含随机，所以各客户端结果一致，多人安全。
/// </summary>
[HarmonyPatch(typeof(RunManager), "GenerateMap")]
public static class Act4MapRegenPatch
{
    /// <summary>第四幕的 act index。</summary>
    private const int Act4Index = 3;

    [HarmonyPrefix]
    private static void Prefix(RunManager __instance)
    {
        __instance.SavedMapsToLoad?.Remove(Act4Index);
    }
}
