using System;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace peak.Core.Visuals;

/// <summary>
/// 战斗中把某个生物节点的视觉场景整个换成另一个
/// （例：领队二阶段从 creature_visuals/leader_miles 换成 creature_visuals/leader_angry）。
///
/// 游戏本体没有提供"中途换皮"的 API：NCreature.Visuals 是 public 属性 + private set，
/// 视觉实例在 NCreature.Create 里由 entity.CreateVisuals() 生成、并在 _Ready 中作为第 0 个子节点插入；
/// 之后 _Ready 里那套装配（bounds / 意图锚点 / 血条锚点 / phobia mode）不会自动重跑。
/// 所以这里做三件事：
///   1) 把新场景挂进节点树，并反射写回 Visuals，让其它读 Visuals 的逻辑指向新场景；
///   2) 反射重跑 UpdateBounds / UpdatePhobiaMode，刷新点击热区、意图位置、血条锚点；
///   3) 摘掉旧场景节点。
///
/// 注意：换完之后游戏发给视觉层的动画触发（NCreature.SetAnimationTrigger → NCreatureVisuals）
/// 只会作用于新场景。若新场景里没有对应的动画节点（例如纯静态 Sprite2D），
/// 攻击/受击/死亡动画会静默失效，但不会报错。
/// </summary>
public static class CreatureVisualSwapper
{
    private static readonly PropertyInfo VisualsProperty =
        typeof(NCreature).GetProperty(nameof(NCreature.Visuals));

    private static readonly MethodInfo UpdateBoundsMethod =
        typeof(NCreature).GetMethod(
            "UpdateBounds",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(Godot.Node) },
            modifiers: null);

    private static readonly MethodInfo UpdatePhobiaModeMethod =
        typeof(NCreature).GetMethod(
            "UpdatePhobiaMode",
            BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>按实体在战斗房间里找到对应的 NCreature；找不到返回 null。</summary>
    public static NCreature FindCreatureNode(Creature entity)
    {
        NCombatRoom room = NCombatRoom.Instance;
        if (room == null || entity == null)
        {
            return null;
        }

        foreach (NCreature candidate in room.CreatureNodes)
        {
            if (candidate.Entity == entity)
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// 把 <paramref name="node"/> 的视觉整场景替换为 <paramref name="innerScenePath"/> 指向的场景。
    /// </summary>
    /// <param name="node">目标生物节点（用 <see cref="FindCreatureNode"/> 拿）。</param>
    /// <param name="innerScenePath">SceneHelper 的"内层路径"，例如 "creature_visuals/leader_angry"。</param>
    /// <returns>替换成功返回 true；节点无效、取不到 setter、场景加载失败等返回 false（不抛异常）。</returns>
    public static bool Swap(NCreature node, string innerScenePath)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
        {
            GD.PushWarning("[CreatureVisualSwapper] 目标节点无效，跳过换皮。");
            return false;
        }

        MethodInfo setter = VisualsProperty == null ? null : VisualsProperty.GetSetMethod(nonPublic: true);
        if (setter == null)
        {
            GD.PushError("[CreatureVisualSwapper] 取不到 NCreature.Visuals 的 setter（游戏版本可能变了），跳过换皮。");
            return false;
        }

        // 先把新场景实例化出来，失败了整体不动，不会留下半截状态
        NCreatureVisuals fresh;
        try
        {
            fresh = SceneHelper.Instantiate<NCreatureVisuals>(innerScenePath);
        }
        catch (Exception ex)
        {
            GD.PushError($"[CreatureVisualSwapper] 加载场景 '{innerScenePath}' 失败：{ex}");
            return false;
        }

        if (fresh == null)
        {
            GD.PushError($"[CreatureVisualSwapper] 场景 '{innerScenePath}' 实例化结果为空，跳过换皮。");
            return false;
        }

        NCreatureVisuals old = node.Visuals;

        try
        {
            // 1) 挂进树，插到第 0 层（与 NCreature._Ready 里的顺序一致）；
            //    此时 fresh 的 _Ready 会同步跑完，%Bounds/%IntentPos/%CenterPos 等唯一名节点就位
            node.AddChildSafely(fresh);
            node.MoveChildSafely(fresh, 0);
            fresh.Position = Vector2.Zero;

            // 2) 换指针：之后所有读 node.Visuals 的逻辑都指向新场景
            setter.Invoke(node, new object[] { fresh });

            // 3) 重跑 _Ready 里那套装配：点击热区、意图位置、血条锚点
            if (UpdateBoundsMethod != null)
            {
                UpdateBoundsMethod.Invoke(node, new object[] { fresh });
            }

            if (UpdatePhobiaModeMethod != null)
            {
                UpdatePhobiaModeMethod.Invoke(node, null);
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"[CreatureVisualSwapper] 替换为 '{innerScenePath}' 时出错：{ex}");
            return false;
        }

        // 4) 新场景已经就位并接管，再摘掉旧的
        if (old != null && GodotObject.IsInstanceValid(old))
        {
            node.RemoveChild(old);
            old.QueueFree();
        }

        GD.Print($"[CreatureVisualSwapper] 视觉已替换为 {innerScenePath}");
        return true;
    }
}
