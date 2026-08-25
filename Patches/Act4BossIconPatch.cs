using System.Reflection;
using HarmonyLib;
using MegaCrit.sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.Acts;

namespace peak.Patches;

/// <summary>
/// 第4幕Boss图标补丁。
/// NTopBarBossIcon.RefreshBossIcon() 用 modelId 拼路径加载图标，
/// 但 Act4 没有对应的 run_history 图片资源，跳过加载避免报错。
/// </summary>
[HarmonyPatch]
public static class Act4BossIconPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(NTopBarBossIcon), "RefreshBossIcon");
    }

    [HarmonyPrefix]
    static bool Prefix(NTopBarBossIcon __instance)
    {
        var runStateField = typeof(NTopBarBossIcon).GetField("_runState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (runStateField?.GetValue(__instance) is not IRunState runState)
            return true;
        if (runState.Act is not Act4)
            return true;

        // Act4没有对应run_history图标资源，跳过加载避免报错
        return false;
    }
}