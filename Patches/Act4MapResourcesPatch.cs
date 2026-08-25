using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Acts;
using System.Reflection;

namespace peak.Patches;

/// <summary>
/// 第4幕地图顶部背景重定向 → glory。
/// </summary>
[HarmonyPatch]
public static class Act4MapTopBgPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ActModel), "MapTopBgPath");
    }

    [HarmonyPostfix]
    static void Postfix(ActModel __instance, ref string __result)
    {
        if (__instance is Act4)
            __result = "res://images/packed/map/map_bgs/glory/map_top_glory.png";
    }
}

/// <summary>
/// 第4幕地图中部背景重定向 → glory。
/// </summary>
[HarmonyPatch]
public static class Act4MapMidBgPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ActModel), "MapMidBgPath");
    }

    [HarmonyPostfix]
    static void Postfix(ActModel __instance, ref string __result)
    {
        if (__instance is Act4)
            __result = "res://images/packed/map/map_bgs/glory/map_middle_glory.png";
    }
}

/// <summary>
/// 第4幕地图底部背景重定向 → glory。
/// </summary>
[HarmonyPatch]
public static class Act4MapBotBgPatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ActModel), "MapBotBgPath");
    }

    [HarmonyPostfix]
    static void Postfix(ActModel __instance, ref string __result)
    {
        if (__instance is Act4)
            __result = "res://images/packed/map/map_bgs/glory/map_bottom_glory.png";
    }
}

/// <summary>
/// 第4幕战斗场景背景路径重定向 → glory。
/// BackgroundScenePath 由 ActModel 根据 FilePathIdentifier 生成，
/// 指向 res://scenes/backgrounds/act4/act4_background.tscn（不存在）。
/// </summary>
[HarmonyPatch]
public static class Act4BackgroundScenePatch
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ActModel), "BackgroundScenePath");
    }

    [HarmonyPostfix]
    static void Postfix(ActModel __instance, ref string __result)
    {
        if (__instance is Act4)
            __result = "res://scenes/backgrounds/glory/glory_background.tscn";
    }
}