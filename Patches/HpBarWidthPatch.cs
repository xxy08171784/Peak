using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Characters;

namespace peak.Patches;

/// <summary>
/// 修正 Scout 与第 4 幕 Boss 的**场内**血条长度。
///
/// ── 原版机制（反编译确认）──
/// 血条容器宽度 HpBarContainer.Size.X 在 mod 触及的范围内只有两个写手：
///   A. <see cref="NHealthBar.UpdateLayoutForCreatureBounds"/>（NHealthBar.cs:126-134）：
///      宽度 = 角色视觉 %Bounds 宽 + (24 - HpBarSizeReduction)，**与血上限无关**。
///      ⚠ 对玩家，_creature.Monster 为 null，可空 float 的 GetValueOrDefault() 返回 0，
///      所以玩家那条实际是 +0，宽度就是裸的 %Bounds 宽（scout.tscn = 170）。
///   B. <see cref="NHealthBar.UpdateWidthRelativeToReferenceValue"/>（NHealthBar.cs:142-147）：
///      宽度 = MaxHp / refMaxHp * refWidth，是唯一按血上限缩放的公式。
///      玩家 HUD 面板走的就是这条（NMultiplayerPlayerState.cs:294，80 血 → 175px）。
///
/// ── 之前的 bug ──
/// 本补丁原来用 B 给 Scout 铺宽度，但 A 是 CallDeferred（帧末）执行的（NHealthBar.cs:153-159），
/// 而 B 是立即执行 —— 同一帧里 B 的结果**一定被 A 覆盖**。
/// 表现：血条常态被钉在 170；一旦 MaxHpChanged（例如混沌的 -5 上限）触发 B，血条会跳到
/// MaxHp/75*240（无上限），而且战斗中再没有任何东西会重跑 A 来纠正它，
/// 于是会一路卡到下一场战斗重建血条为止。Boss 那条强制 420 同样被 A 覆盖，从未真正生效。
///
/// ── 现在的做法 ──
/// 不再跟 A/B 抢先后，而是在两者的**必经之路**上加一道闸：prefix 拦截私有的
/// NHealthBar.SetHpBarContainerSizeWithOffsetsImmediately(ref size)，直接把 size.X 改写成
/// 由 MaxHp 推导并钳制后的值。两个写手因此算出同一个结果，帧时序不再影响宽度。
/// </summary>
[HarmonyPatch]
public static class HpBarWidthPatch
{
    // Scout 基准：初始血上限 75 → 240px。240 对齐原版铁甲战士的场内血条宽度（~239px）。
    private const float ScoutRefMaxHp = 75f;
    private const float ScoutRefWidth = 240f;

    // Scout 血条宽度上下限：下限防血上限被压到 1 时血条缩成一条线，上限防爆长。
    private const float ScoutMinWidth = 60f;
    private const float ScoutMaxWidth = 420f;

    // 第 4 幕 Boss 血条的最小宽度。
    private const float Act4BossWidth = 420f;

    // 0-based：第 4 幕 = index 3。
    private const int Act4Index = 3;

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> _creatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");

    /// <summary>
    /// 唯一的宽度执法点。
    ///
    /// <c>NHealthBar.SetHpBarContainerSizeWithOffsetsImmediately(Vector2 size)</c> 是 private，
    /// 只能按名字字符串打补丁（游戏版本更新改了名字就会静默失效，届时血条退回原版行为）。
    /// 它同时被 UpdateLayoutForCreatureBounds（帧末 deferred）与
    /// UpdateWidthRelativeToReferenceValue（立即）调用 —— 拦在这里，两个写手就再也打不起来。
    /// 只改 size.X，size.Y 沿用调用方传进来的当前高度。
    /// </summary>
    [HarmonyPatch(typeof(NHealthBar), "SetHpBarContainerSizeWithOffsetsImmediately")]
    [HarmonyPrefix]
    static void OnSetHpBarContainerSize(NHealthBar __instance, ref Vector2 size)
    {
        var creature = _creatureRef(__instance);
        if (creature == null || !IsInCombatBar(__instance))
        {
            return;
        }

        // 玩家（Scout）：宽度只由血上限决定，钳制在 [60, 420]。
        // 注意这里**不读**传进来的 size.X —— 正因如此，无论 A 还是 B 调进来算出的都是同一个值，
        // 帧时序不再影响结果。
        if (IsScoutPlayer(creature))
        {
            size.X = Math.Clamp(creature.MaxHp / ScoutRefMaxHp * ScoutRefWidth,
                                ScoutMinWidth, ScoutMaxWidth);
            return;
        }

        // 第 4 幕 Boss：占位场景沿用了 Scout 的小 Bounds（170），血条明显偏短 —— 强制最小宽度。
        // 取 Max 而非直接赋值，保留"只加长、不缩短"的原意。
        if (creature.IsPrimaryEnemy && IsAct4BossRoom(creature))
        {
            size.X = Math.Max(size.X, Act4BossWidth);
        }
    }

    [HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.SetCreature))]
    [HarmonyPostfix]
    static void OnSetCreature(NHealthBar __instance, Creature creature)
    {
        if (creature == null || !IsScoutPlayer(creature) || !IsInCombatBar(__instance))
        {
            return;
        }

        // 玩家角色是整局常驻对象，而 NHealthBar 每场战斗重建一次。
        // 若只 += 不退订，每打一场就往玩家身上多挂一个 handler，而这些 handler 抓着已释放的血条
        // ——纯泄漏（原来就是这样）。现在把退订绑在血条自己的 TreeExiting 信号上
        // （同原版 NodeUtil / TweenHelper 的清理手法），血条离开场景树时精确摘掉。
        Action<int, int> handler = null;
        handler = (_, _) =>
        {
            if (!GodotObject.IsInstanceValid(__instance))
            {
                creature.MaxHpChanged -= handler; // 兜底自摘
                return;
            }

            ApplyScoutWidth(__instance, creature);
        };

        creature.MaxHpChanged += handler;
        __instance.TreeExiting += () => creature.MaxHpChanged -= handler;

        ApplyScoutWidth(__instance, creature);
    }

    /// <summary>触发一次宽度重算；最终写入的值由 <see cref="OnSetHpBarContainerSize"/> 钳制决定。</summary>
    private static void ApplyScoutWidth(NHealthBar bar, Creature creature)
        => bar.UpdateWidthRelativeToReferenceValue(ScoutRefMaxHp, ScoutRefWidth);

    private static bool IsScoutPlayer(Creature creature)
        => creature.IsPlayer && creature.Player?.Character is Scout;

    /// <summary>
    /// 只作用于场内血条（NHealthBar → NCreatureStateDisplay → NCreature）。
    /// 玩家 HUD 面板里的 NHealthBar 不在 NCreature 下，因此天然被排除
    /// （那条本来就由原版按血上限缩放，不该被这里覆盖）。
    /// </summary>
    private static bool IsInCombatBar(NHealthBar bar)
        => bar.GetParent() is NCreatureStateDisplay stateDisplay
        && stateDisplay.GetParent() is NCreature;

    private static bool IsAct4BossRoom(Creature creature)
    {
        var runState = creature.CombatState?.RunState;
        return runState != null
            && runState.CurrentActIndex == Act4Index
            && runState.CurrentRoom is CombatRoom room
            && room.RoomType == RoomType.Boss;
    }
}
