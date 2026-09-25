using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace peak.Core.Endings;

/// <summary>
/// 特殊结局 CG 播放层：全屏依次播放 images/endings/end1..3.png。
///
/// 挂在根节点下的高层 CanvasLayer 里（见 <see cref="SpecialEndingState.RunAsync"/>）。
/// CanvasLayer 下没有 Control 父级，所以本节点的锚点直接按视口解析，FullRect 即满屏，
/// 不依赖任何上层控件的布局。
///
/// 节奏：每张默认播 2 秒自动切下一张；期间按任意键 / 点击立即跳下一张；
/// 跳到最后一张（或最后一张自动播完）后，再按任意键 / 点击即退出（触发 <see cref="Finished"/>）。
/// </summary>
public sealed partial class EndingCgOverlay : Control
{
    /// <summary>每张图的自动停留时长（秒）。</summary>
    private const double SecondsPerImage = 2.0;

    private static readonly string[] InnerPaths =
    {
        "endings/end1.png",
        "endings/end2.png",
        "endings/end3.png",
    };

    private readonly List<Texture2D> _textures = new();

    private ColorRect? _back;
    private TextureRect? _image;
    private int _index;
    private double _elapsed;
    private bool _layoutLogged;

    /// <summary>最后一张已经播完，正在等玩家输入退出。</summary>
    private bool _playedAll;

    /// <summary>已经退出，避免重复触发。</summary>
    private bool _closing;

    /// <summary>CG 播完且玩家确认退出时触发（由 SpecialEndingState.RunAsync 订阅，随后调 WinRun）。</summary>
    public event Action? Finished;

    public override void _Ready()
    {
        // CanvasLayer 下没有 Control 父级，这个 Godot 定制版里锚点解析不出尺寸，
        // 必须显式按视口尺寸铺满（视口 1920x1080，CG 图也是 1920x1080，正好满屏）。
        Vector2 vp = GetViewportRect().Size;
        Size = vp;
        MouseFilter = MouseFilterEnum.Stop;
        SetProcess(true);
        SetProcessInput(true);

        // 纯黑底：非 16:9 屏幕留边时不露出底层界面
        var back = new ColorRect
        {
            Color = Colors.Black,
            Size = vp,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(back);
        _back = back;

        foreach (string inner in InnerPaths)
        {
            Texture2D? texture = GD.Load<Texture2D>(ImageHelper.GetImagePath(inner));
            if (texture != null)
            {
                _textures.Add(texture);
                GD.Print($"[EndingCg] 已加载 {inner} ({texture.GetWidth()}x{texture.GetHeight()})");
            }
            else
            {
                GD.PushWarning($"[EndingCg] 载入失败，跳过：{ImageHelper.GetImagePath(inner)}");
            }
        }

        _image = new TextureRect
        {
            Texture = _textures.Count > 0 ? _textures[0] : null,
            Size = vp,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_image);

        // 一张都拿不到就不要卡住玩家：直接进入"等输入退出"
        _playedAll = _textures.Count == 0;
        GD.Print($"[EndingCg] 就绪，共 {_textures.Count} 张，Size={Size}");
    }

    public override void _Process(double delta)
    {
        if (_closing)
        {
            return;
        }

        // 尺寸靠显式赋值（锚点在这个父级链下不生效），窗口变化时跟随
        Vector2 vp = GetViewportRect().Size;
        if (Size != vp)
        {
            Size = vp;
        }

        if (_back != null && _back.Size != vp)
        {
            _back.Size = vp;
        }

        if (_image != null && _image.Size != vp)
        {
            _image.Size = vp;
        }

        // 只打一次，用于确认真的铺满了（排查"看不见 CG"用）
        if (!_layoutLogged)
        {
            _layoutLogged = true;
            GD.Print($"[EndingCg] 布局后 Size={Size}，视口={vp}");
        }

        if (_playedAll)
        {
            return;
        }

        _elapsed += delta;
        if (_elapsed < SecondsPerImage)
        {
            return;
        }

        _elapsed = 0;
        ShowNext();
    }

    public override void _Input(InputEvent @event)
    {
        if (_closing)
        {
            return;
        }

        bool confirm = @event is InputEventKey { Pressed: true }
            or InputEventMouseButton { Pressed: true };
        if (!confirm)
        {
            return;
        }

        GetViewport()?.SetInputAsHandled();

        if (_playedAll)
        {
            Close();
            return;
        }

        // 手动提前：跳下一张。跳完最后一张就直接退出，不让玩家多点一次。
        if (_index >= _textures.Count - 1)
        {
            Close();
            return;
        }

        _elapsed = 0;
        _index++;
        if (_image != null)
        {
            _image.Texture = _textures[_index];
        }

        GD.Print($"[EndingCg] 手动跳到第 {_index + 1} 张");
    }

    /// <summary>自动到点：推进一张；已经是最后一张则进入等待退出。</summary>
    private void ShowNext()
    {
        if (_index >= _textures.Count - 1)
        {
            _playedAll = true;
            GD.Print("[EndingCg] 三张播完，等待任意键退出");
            return;
        }

        _index++;
        if (_image != null)
        {
            _image.Texture = _textures[_index];
        }

        GD.Print($"[EndingCg] 自动切到第 {_index + 1} 张");
    }

    private void Close()
    {
        _closing = true;
        GD.Print("[EndingCg] 玩家确认退出");
        Finished?.Invoke();
    }
}
