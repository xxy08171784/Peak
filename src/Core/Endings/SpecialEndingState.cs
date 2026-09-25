using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace peak.Core.Endings;

/// <summary>
/// 特殊结局（宾邦好结局）的状态与流程。
///
/// 触发链：
///   宾邦「销毁」自爆 → <see cref="Binbang"/> 调 <see cref="MarkBinbangDefeated"/> 置位
///   → 玩家领完 Boss 奖励、点继续 → ActChangeSynchronizer → RunManager.EnterNextAct
///   → SpecialEndingPatch 命中 → 本类 <see cref="RunAsync"/> 接管：
///   清屏 → 播 CG → RunManager.WinRun()（成功结算），跳过见建筑师。
///
/// 结算横幅由 TrueEndingBannerPatch 读 <see cref="Triggered"/>，结算屏出现后调 <see cref="Reset"/> 清空，
/// 避免标记泄漏到下一局（例如宾邦自爆后玩家反而团灭的失败局）。
/// </summary>
public static class SpecialEndingState
{
    /// <summary>CG 所在 CanvasLayer 的层级，压过游戏所有常规 UI。</summary>
    private const int CgLayerIndex = 100;

    /// <summary>宾邦已最终死亡（DestroyMove 置位），本局战斗确认为好结局。</summary>
    public static bool BinbangDefeated { get; private set; }

    /// <summary>本次结算要走真结局横幅（EnterNextAct 命中时置位）。</summary>
    public static bool Triggered { get; private set; }

    public static void MarkBinbangDefeated()
    {
        BinbangDefeated = true;
    }

    /// <summary>
    /// EnterNextAct 命中时调用：消费宾邦标记并置位真结局标记。
    /// 返回 true 表示本次应走特殊结局流程。
    /// </summary>
    public static bool TryConsumeForEnding()
    {
        if (!BinbangDefeated)
        {
            return false;
        }

        BinbangDefeated = false;
        Triggered = true;
        return true;
    }

    /// <summary>结算屏出现后清空全部状态，避免泄漏到下一局。</summary>
    public static void Reset()
    {
        BinbangDefeated = false;
        Triggered = false;
    }

    /// <summary>清屏 → 播 CG → 等玩家看完 → WinRun（成功结算）。</summary>
    public static async Task RunAsync()
    {
        // 再等一帧：调用方已用 CallDeferred，这里是双保险，
        // 确保奖励屏那条点击回调栈完全展开后再动 UI。
        NRun? run = NRun.Instance;
        if (run != null)
        {
            await NodeUtil.AwaitProcessFrame(run, CancellationToken.None);
        }

        // 关掉奖励屏等覆盖层（等同 RunManager.ClearScreens 的三步）
        NOverlayStack.Instance?.Clear();
        NCapstoneContainer.Instance?.Close();
        NMapScreen.Instance?.Close(animateOut: false);

        // CG 走 CanvasLayer 挂在根节点上：层级最高、Control 的锚点直接按视口解析，
        // 不依赖 NOverlayStack 的布局（上一版挂在覆盖层栈里，CG 没能显示出来）。
        Node? parent = NRun.Instance ?? (Node?)NGame.Instance;
        if (parent == null)
        {
            GD.PushError("[SpecialEnding] 找不到挂载节点，直接 WinRun 结算（不播 CG）");
            await RunManager.Instance.WinRun();
            return;
        }

        var layer = new CanvasLayer { Layer = CgLayerIndex };
        var overlay = new EndingCgOverlay();
        layer.AddChild(overlay);
        parent.AddChild(layer);
        GD.Print($"[SpecialEnding] CG 层已挂载到 '{parent.Name}'，CanvasLayer.Layer={CgLayerIndex}");

        var done = new TaskCompletionSource<bool>();
        overlay.Finished += () =>
        {
            // 先隐藏再释放，避免结算屏弹出的那一帧出现叠影
            layer.Hide();
            layer.QueueFree();
            done.TrySetResult(true);
        };

        await done.Task;
        await RunManager.Instance.WinRun();
    }
}
