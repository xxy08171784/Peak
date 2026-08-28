using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Patches;

/// <summary>
/// 修复第4幕（actIndex=3）在多人模式中的 HP 缩放异常。
/// 
/// 游戏原版的 MultiplayerScalingModel.GetMultiplayerScaling() 只定义了 actIndex=0,1,2，
/// actIndex >= 3 时抛出 ArgumentOutOfRangeException，导致进入四层Boss房间时黑屏崩溃。
/// 
/// 本补丁拦截 GetMultiplayerScaling，当 actIndex >= 3 时返回合理的缩放系数。
/// </summary>
[HarmonyPatch(typeof(MultiplayerScalingModel), "GetMultiplayerScaling")]
public static class MultiplayerScalingPatch
{
    [HarmonyPrefix]
    static bool Prefix(ref decimal __result, EncounterModel? encounter, int actIndex)
    {
        // 只拦截 actIndex >= 3 的情况（第4幕及以上），原逻辑不变
        if (actIndex < 3) return true;

        // 第4幕缩放系数：
        // - Boss房：1.5x（比第3幕Boss的1.3x更高）
        // - 普通房：1.3x（比第3幕普通的1.2x略高）
        // - 其他：1.3x
        if (actIndex == 3)
        {
            if (encounter != null && encounter.RoomType == RoomType.Boss)
            {
                __result = 1.5m;
            }
            else
            {
                __result = 1.3m;
            }
        }
        else
        {
            // actIndex > 3 的兜底（如未来有更多自定义层）
            __result = 1.3m;
        }

        return false; // 跳过原方法
    }
}