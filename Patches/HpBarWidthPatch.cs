using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Characters;

namespace peak.Patches;

/// <summary>
/// 修正 Scout 与第 4 幕 Boss 的场内血条长度。
///
/// 原版机制（反编译 sts2.dll 确认）：
///  - 场内血条：<see cref="NHealthBar.UpdateLayoutForCreatureBounds"/>，
///    宽度 = 角色视觉 %Bounds 宽度（+24 再减 HpBarSizeReduction），与血上限无关。
///    原版角色的 %Bounds 显然比 Scout 的 170 宽：铁甲战士 239 / 静默 363 / 缺陷 357。
///  - 玩家 HUD 面板：<see cref="NHealthBar.UpdateWidthRelativeToReferenceValue"/>（80 血 → 175px），
///    只有这一条才按血上限缩放。
///
/// Scout 是逐帧动画角色（非 Spine），BoundsUpdated 信号永不触发，UpdateBounds 只在 _Ready 跑一次，
/// 所以血条被永久钉死在场景里写死的 170px，血上限涨了也不会变。
/// 这里改成用原版 HUD 的参考公式驱动，并订阅 MaxHpChanged，使血条随血上限实时变长。
/// </summary>
[HarmonyPatch]
public static class HpBarWidthPatch
{
    // Scout 基准：初始血上限 75 → 240px。240 对齐原版铁甲战士的场内血条宽度（~239px）。
    private const float ScoutRefMaxHp = 75f;
    private const float ScoutRefWidth = 240f;

    // 第 4 幕 Boss 血条的最小宽度。
    private const float Act4BossWidth = 420f;

    // 0-based：第 4 幕 = index 3。
    private const int Act4Index = 3;

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> _creatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");

    [HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.SetCreature))]
    [HarmonyPostfix]
    static void OnSetCreature(NHealthBar __instance, Creature creature)
    {
        if (creature == null || !IsScoutPlayer(creature) || !IsInCombatBar(__instance))
        {
            return;
        }

        // 血上限变化时重新按比例铺开（IsInstanceValid 防止血条已释放时回调）
        creature.MaxHpChanged += (_, _) =>
        {
            if (GodotObject.IsInstanceValid(__instance))
            {
                ApplyScoutWidth(__instance, creature);
            }
        };

        ApplyScoutWidth(__instance, creature);
    }

    [HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.UpdateLayoutForCreatureBounds))]
    [HarmonyPostfix]
    static void OnLayoutForCreatureBounds(NHealthBar __instance)
    {
        var creature = _creatureRef(__instance);
        if (creature == null || !IsInCombatBar(__instance))
        {
            return;
        }

        // Scout：覆盖掉场景里写死的 Bounds 宽度，改按血上限缩放。
        if (IsScoutPlayer(creature))
        {
            ApplyScoutWidth(__instance, creature);
            return;
        }

        // 第 4 幕 Boss：占位场景沿用了 Scout 的小 Bounds（170），血条明显偏短 —— 强制最小宽度。
        if (creature.IsPrimaryEnemy && IsAct4BossRoom(creature)
            && __instance.HpBarContainer.Size.X < Act4BossWidth)
        {
            // UpdateWidthRelativeToReferenceValue 算的是 MaxHp / refMaxHp * refWidth，
            // 传 refMaxHp = 当前 MaxHp 即得到固定宽度 refWidth（Boss 血量会变，不能按血上限缩放）。
            __instance.UpdateWidthRelativeToReferenceValue(creature.MaxHp, Act4BossWidth);
        }
    }

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
