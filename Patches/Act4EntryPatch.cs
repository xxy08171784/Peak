using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Models.Acts;
using peak.Core.Models.Encounters;

namespace peak.Patches;

/// <summary>
/// 第4幕进入门槛与注入。
///
/// Act4 通过 <c>CustomActModel(autoAdd:false)</c> 构造，不在 ModelDb.Acts 里，
/// 所以每一局默认只有 3 幕、原版流程完全不受影响。
/// 当第3幕 Boss 战后（CurrentActIndex==2）且集齐4颗宝石时，这里把 Act4 追加进
/// RunState.Acts 并生成房间；随后原版 EnterNextAct 会看到 Acts.Count=4 → 调用 EnterAct(3)。
/// 未集齐宝石则什么都不做，原版 EnterNextAct 正常进入建筑师结算。
///
/// 房间生成用的是共享 RNG（RunRngSet.UpFront），保证多人各客户端结果一致。
/// </summary>
[HarmonyPatch(typeof(RunManager), "EnterNextAct")]
public static class Act4EntryPatch
{
    [HarmonyPrefix]
    static void Prefix(RunManager __instance)
    {
        if (ReadState(__instance) is not RunState state)
        {
            return;
        }

        // 只处理"第3幕 Boss 战后"这一个时机
        if (state.CurrentActIndex != 2)
        {
            return;
        }

        // 防重复注入（例如读档后 Acts 里已含第4幕）
        if (state.Acts.Any(a => a.Index >= 3))
        {
            return;
        }

        if (!Act4GemManagerPatch.HasAllFourGems(state))
        {
            return;
        }

        GD.Print("[Act4] 集齐4颗宝石，注入第4幕...");

        // BaseLib 已把 Act4 注册为 canonical 模型，ToMutable() 直接可用
        // （MutableClone 内部会重置 _rooms，随后的 GenerateRooms 填充房间）。
        var act4 = (Act4)ModelDb.Act<Act4>().ToMutable();
        act4.GenerateRooms(state.Rng.UpFront, state.UnlockState, state.Players.Count > 1);

        // 第四层地图上的"第二个 Boss" = 隐藏 Boss「宾邦」。
        // 节点只会在好结局（拿到童军的荣耀）后出现（见 NadirActMap.SecondBossMapPoint），
        // 这里先登记遭遇战；存 canonical，CreateRoom 时会自行 ToMutable。
#pragma warning disable CS0618 // ModelDb.Encounter<T> 可能标记了 Obsolete
        act4.SetSecondBossEncounter(ModelDb.Encounter<BinbangBoss>());
#pragma warning restore CS0618

        var acts = state.Acts.ToList();
        acts.Add(act4);

        // 唯一需要反射的地方：RunState.Acts 是 public get / private set。
        var actsProp = typeof(RunState).GetProperty("Acts",
            BindingFlags.Instance | BindingFlags.Public);
        if (actsProp == null)
        {
            GD.PrintErr("[Act4] 找不到 RunState.Acts 属性，无法注入第4幕！");
            return;
        }

        actsProp.SetValue(state, acts);

        GD.Print($"[Act4] 已注入，Acts.Count={acts.Count}，EnterNextAct 将进入 EnterAct(3)。");
    }

    /// <summary>
    /// RunManager.State 是 private 属性，需要反射读取。
    /// </summary>
    private static IRunState? ReadState(RunManager runManager)
    {
        var stateProp = typeof(RunManager).GetProperty("State",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return stateProp?.GetValue(runManager) as IRunState;
    }
}
