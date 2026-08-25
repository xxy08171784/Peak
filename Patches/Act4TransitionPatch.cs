using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using peak.Core.Models.Acts;

namespace peak.Patches;

/// <summary>
/// 第4幕进入补丁。
/// 拦截 RunManager.EnterNextAct()：第3幕 Boss 战后（CurrentActIndex==2），
/// 向 Acts 列表注入 Act4，使 Acts.Count=4，EnterNextAct 检测到
/// CurrentActIndex(2) < Acts.Count-1(3) 时进入 EnterAct(3) 而不是建筑师。
/// 第4幕 Boss 战后 CurrentActIndex==3，直接正常进入建筑师。
/// </summary>
[HarmonyPatch(typeof(RunManager), "EnterNextAct")]
public static class Act4TransitionPatch
{
    [HarmonyPrefix]
    static bool Prefix(RunManager __instance)
    {
        // State 是 private auto-property，背后字段名是 <State>k__BackingField
        // 所以必须用 GetProperty 而非 GetField
        var stateProp = typeof(RunManager).GetProperty("State",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (stateProp == null)
        {
            GD.PrintErr("[Act4] CRITICAL: RunManager.State property not found!");
            return true;
        }

        var state = (IRunState)stateProp.GetValue(__instance);
        if (state == null) return true;

        GD.Print("[Act4] EnterNextAct called! CurrentActIndex=" + state.CurrentActIndex +
            ", Acts.Count=" + state.Acts.Count);

        // 新周目开始时重置静态状态
        if (state.CurrentActIndex == 0)
        {
            Act4GemManagerPatch.Internal_FirstEliteGemGranted = false;
            return true;
        }

        // 仅在第3幕结束时注入 (CurrentActIndex == 2)
        if (state.CurrentActIndex != 2) return true;

        // [无条件注入] — 暂时跳过宝石检测，先打通第4幕流程
        // if (!Act4GemManagerPatch.HasAllFourGems(state)) return true;

        GD.Print("[Act4] EnterNextAct intercepted! Injecting Act4...");

        // 将 Act4 注入到 Acts 列表末尾
        var actsProp = typeof(RunState).GetProperty("Acts",
            BindingFlags.Instance | BindingFlags.Public);
        if (actsProp == null) return true;

        var current = (IReadOnlyList<ActModel>)actsProp.GetValue(state);
        if (current == null) return true;

        // 防重复注入
        if (current.Any(a => a.Index >= 3))
        {
            GD.Print("[Act4] Act4 already injected, skipping.");
            return true;
        }

        var list = current.ToList();

        // Act4 已经在 ModelDb 中注册（BaseLib 启动时扫描 AbstractModel 子类注册）。
        // 直接 ModelDb.Act<Act4>().ToMutable() 会触发构造函数 DuplicateModelException，
        // 因为 ToMutable() 内部的某些路径调用了 AbstractModel 构造函数检测。
        // 修复：从 ModelDb 拿 canonical，然后手动调 MutableClone()（用 MemberwiseClone 绕过构造函数）。
        var act4Template = ModelDb.Act<Act4>();
        var act4 = (Act4)act4Template.MutableClone();
        // CanonicalInstance 只读，用反射写私有字段 _canonicalInstance
        typeof(ActModel).GetField("_canonicalInstance", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(act4, act4Template);

        // 获取 Rng (RunRngSet) + UnlockState 用于生成房间
        // RunState.Rng 返回的是 RunRngSet 类型，需要 .UpFront 拿到 Rng
        var rngProp = typeof(RunState).GetProperty("Rng",
            BindingFlags.Instance | BindingFlags.Public);
        var runStateObj = state as RunState;
        Rng rng;
        if (rngProp != null && runStateObj != null)
        {
            var rngSet = (RunRngSet)rngProp.GetValue(runStateObj);
            rng = rngSet.UpFront;
        }
        else
        {
            rng = new Rng(0, "act4_fallback");
        }

        var unlockState = state.Players[0].UnlockState;
        act4.GenerateRooms(rng, unlockState, state.Players.Count > 1);

        list.Add(act4);
        actsProp.SetValue(state, (IReadOnlyList<ActModel>)list);

        GD.Print("[Act4] Act4 injected! Acts.Count=" + list.Count +
            ". EnterNextAct will see 2 < " + (list.Count - 1) + " and go to EnterAct(3).");

        return true; // 让原 EnterNextAct 继续执行
    }
}