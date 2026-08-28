using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using peak.Core.Models.Powers;

namespace peak.Patches;

/// <summary>
/// Scout 三 buff（炎热/孢子/中毒）第二血条。性能优化版：
/// - 所有反射字段提前缓存为静态委托（AccessTools.FieldRefAccess）
/// - 子节点引用一次性缓存（BuffBarCache），每帧不走 GetNodeOrNull
/// - creatureNode 按固定层级直接取（NHealthBar→NCreatureStateDisplay→NCreature，不走循环遍历）
/// - 删除每帧 GD.Print 日志洪水（只保留创建时的单次日志）
/// - 其余逻辑不变
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
public static class HpBarBuffOverlayPatch
{
    // 缓存的 _creature 字段访问委托（FieldRef 结构体，Invoke 不涉及反射）
    private static readonly AccessTools.FieldRef<NHealthBar, Creature> _creatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");

    // —— 容量与几何 ——
    private const int Cap = 100;
    private const float BarH = 12f;
    private const float Gap = 4f;
    private const float Inset = 10f;
    private const float FallbackWidth = 175f;

    // —— 颜色 ——
    private static readonly Color HeatCol = new Color(1f, 0.52f, 0.08f);
    private static readonly Color SporeCol = new Color(0.55f, 0.72f, 0.48f);
    private static readonly Color PoisonCol = new Color(0.85f, 0.48f, 1f);
    private static readonly Color BgCol = new Color(0f, 0f, 0f, 0.65f);
    private static readonly Color BorderCol = new Color(1f, 1f, 1f, 0.9f);

    // —— 几何常量 ——
    private const float SegGap = 2f;
    private const float BorderW = 1.5f;
    private const string BarNodeName = "ScoutBuffBarRoot";

    // —— 子节点引用缓存（静态字段，只有一个本地玩家，安全） ——
    // 当副血条被创建时写入；当 Postfix 发现 bar 已存在时复用。
    // Combat 结束后 NCreature→bar 一起被 free，下一场战斗 Postfix 重建 bar 并重新缓存。
    private static BuffBarCache? _barCache;

    private sealed class BuffBarCache
    {
        public readonly ColorRect Bg;
        public readonly ColorRect Heat;
        public readonly ColorRect Spore;
        public readonly ColorRect Poison;
        public readonly ColorRect BdT;
        public readonly ColorRect BdB;
        public readonly ColorRect BdL;
        public readonly ColorRect BdR;
        public readonly Label TotalLabel;

        public BuffBarCache(Control bar)
        {
            Bg = bar.GetNode<ColorRect>("ScoutBuffBg");
            Heat = bar.GetNode<ColorRect>("ScoutBuffHeat");
            Spore = bar.GetNode<ColorRect>("ScoutBuffSpore");
            Poison = bar.GetNode<ColorRect>("ScoutBuffPoison");
            BdT = bar.GetNode<ColorRect>("ScoutBuffBdT");
            BdB = bar.GetNode<ColorRect>("ScoutBuffBdB");
            BdL = bar.GetNode<ColorRect>("ScoutBuffBdL");
            BdR = bar.GetNode<ColorRect>("ScoutBuffBdR");
            TotalLabel = bar.GetNode<Label>("ScoutBuffTotal");
        }
    }

    static void Postfix(NHealthBar __instance)
    {
        try
        {
            // 1. 用缓存委托读 _creature（无反射 GetValue 开销）
            var creature = _creatureRef(__instance);
            if (creature == null || !creature.IsPlayer || !LocalContext.IsMe(creature))
            {
                return;
            }

            // 2. 血条层级固定：NHealthBar → NCreatureStateDisplay → NCreature
            //    直接 GetParent 两次，不遍历。多人 HUD 里的 NHealthBar 不在 NCreature 下，
            //    GetParent 两次后不是 NCreature → 自然过滤。
            var stateDisplay = __instance.GetParent() as NCreatureStateDisplay;
            var creatureNode = stateDisplay?.GetParent() as NCreature;
            if (creatureNode == null)
            {
                return;
            }

            // 3. 懒创建副血条，并首次懒初始化子节点缓存
            var bar = creatureNode.GetNodeOrNull<Control>(BarNodeName);
            if (bar == null)
            {
                bar = CreateBar();
                creatureNode.AddChild(bar);
                _barCache = new BuffBarCache(bar);
                GD.Print($"[BuffBar] CREATED under {creatureNode.GetPath()}");
            }
            var cache = _barCache;
            if (cache == null)
            {
                return;
            }

            // 4. 定位：血条全局矩形 → 全局坐标差（引擎绑定裁剪，无 ToLocal）
            var hpRect = __instance.HpBarContainer?.GetGlobalRect() ?? new Rect2();
            float barW = hpRect.Size.X > 20f ? hpRect.Size.X - 2f * Inset : FallbackWidth - 2f * Inset;
            bar.Position = (hpRect.Position + new Vector2(Inset, -Gap - BarH)) - creatureNode.GlobalPosition;
            bar.Size = new Vector2(barW, BarH);

            // 5. 数值
            int heat = creature.GetPowerAmount<HeatPower>();
            int spore = creature.GetPowerAmount<SporePower>();
            int poison = creature.GetPowerAmount<ZhongduPower>();
            int total = heat + spore + poison;
            bool show = total > 0 && creature.CurrentHp > 0;

            // 6. 更新子节点（全用缓存引用，零场景树查找）
            cache.Bg.Size = new Vector2(barW, BarH);
            cache.Bg.Visible = show;

            float x = 0f;
            x = SetSeg(cache.Heat, heat, barW, x, HeatCol);
            x = SetSeg(cache.Spore, spore, barW, x, SporeCol);
            x = SetSeg(cache.Poison, poison, barW, x, PoisonCol);

            // 白色边框
            cache.BdT.Position = new Vector2(0f, 0f);
            cache.BdT.Size = new Vector2(barW, BorderW);
            cache.BdB.Position = new Vector2(0f, BarH - BorderW);
            cache.BdB.Size = new Vector2(barW, BorderW);
            cache.BdL.Position = new Vector2(0f, 0f);
            cache.BdL.Size = new Vector2(BorderW, BarH);
            cache.BdR.Position = new Vector2(barW - BorderW, 0f);
            cache.BdR.Size = new Vector2(BorderW, BarH);

            // 总和小字
            cache.TotalLabel.Text = $"{total}/{Cap}";
            cache.TotalLabel.Position = new Vector2(barW + 4f, BarH - 18f);
            cache.TotalLabel.Visible = show;

            bar.Visible = show;
        }
        catch (Exception e)
        {
            GD.PrintErr($"[BuffBar] ERROR: {e}");
        }
    }

    private static float SetSeg(ColorRect seg, int amount, float barW, float x, Color color)
    {
        float w = barW * (amount / (float)Cap);
        if (amount > 0 && w > SegGap)
        {
            w -= SegGap;
        }
        seg.Color = color;
        seg.Position = new Vector2(x, 0f);
        seg.Size = new Vector2(Math.Max(w, amount > 0 ? 2f : 0f), BarH);
        seg.Visible = amount > 0;
        return x + w + (amount > 0 ? SegGap : 0f);
    }

    private static Control CreateBar()
    {
        var bar = new Control
        {
            Name = BarNodeName,
            ZIndex = 10,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        bar.AddChild(new ColorRect { Name = "ScoutBuffBg",     Color = BgCol,     MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffHeat",   Color = HeatCol,   MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffSpore",  Color = SporeCol,  MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffPoison", Color = PoisonCol, MouseFilter = Control.MouseFilterEnum.Ignore });

        bar.AddChild(new ColorRect { Name = "ScoutBuffBdT", Color = BorderCol, MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffBdB", Color = BorderCol, MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffBdL", Color = BorderCol, MouseFilter = Control.MouseFilterEnum.Ignore });
        bar.AddChild(new ColorRect { Name = "ScoutBuffBdR", Color = BorderCol, MouseFilter = Control.MouseFilterEnum.Ignore });

        var label = new Label { Name = "ScoutBuffTotal", Text = "", ZIndex = 1, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        bar.AddChild(label);

        return bar;
    }
}