using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Patches;

/// <summary>
/// 保护「被抛弃者」Power 不被任何清除效果移除。
/// 拦截 PowerCmd.Remove(PowerModel)，如果目标 Power 是 AbandonedMarkPower 则跳过。
/// 注意：PowerCmd.Remove 有泛型重载 Remove&lt;T&gt;(Creature)，必须显式指定参数类型，
/// 否则 Harmony 会因重载歧义抛 AmbiguousMatchException。
/// </summary>
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), new[] { typeof(PowerModel) })]
public static class AbandonedMarkProtectionPatch
{
    [HarmonyPrefix]
    static bool PreventAbandonedMarkRemoval(PowerModel power)
    {
        if (power is AbandonedMarkPower)
        {
            // 被抛弃者不能被清除，直接跳过
            return false;
        }
        return true;
    }
}