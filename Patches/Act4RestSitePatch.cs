using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;

namespace peak.Patches;

/// <summary>
/// 篝火角色动画兜底（原版限制的唯一 workaround）。
///
/// 原版 <c>NRestSiteCharacter._Ready()</c> 里
/// <c>switch (Player.RunState.CurrentActIndex) { 0/1/2 ... _ =&gt; throw "Unexpected act" }</c>
/// 只处理 act 0/1/2，第4幕 index=3 会直接抛异常。
///
/// 这里**不修改** RunState.CurrentActIndex —— 那样会触发它的 setter 清空已访问地图坐标
/// （旧实现正是因此才需要一整套 _visitedMapCoords 备份/还原补丁）。
/// 改为在 _Ready 执行期间用一个作用域标志，把 CurrentActIndex 的读取结果 3 映射为 2，
/// 让原方法走 glory_loop 分支；_Ready 结束后标志失效，其余逻辑读到的仍是 3。
/// </summary>
[HarmonyPatch]
public static class Act4RestSitePatch
{
    private static bool _inRestSiteReady;

    [HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
    [HarmonyPrefix]
    private static void RestSiteReady_Prefix() => _inRestSiteReady = true;

    [HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
    [HarmonyPostfix]
    private static void RestSiteReady_Postfix() => _inRestSiteReady = false;

    [HarmonyPatch(typeof(RunState), nameof(RunState.CurrentActIndex), MethodType.Getter)]
    [HarmonyPostfix]
    private static void CurrentActIndex_Postfix(ref int __result)
    {
        // 第4幕(index 3)的篝火角色复用 act3 的动画
        if (_inRestSiteReady && __result == 3)
        {
            __result = 2;
        }
    }
}
