using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;
using peak.Core.Endings;

namespace peak.Patches;

/// <summary>
/// 特殊结局：拦截 RunManager.EnterNextAct。
///
/// EnterNextAct 在最后一幕会进入建筑师房间（EventRoom(TheArchitect)）——那就是原版的结算入口。
/// 原版在这条分支里自己写了 "WinRun should be called instead" 的异常信息，
/// 因为建筑师事件演完的最后一步就是 await RunManager.Instance.WinRun()。
///
/// 所以这里做的事就是：命中（本局宾邦已最终死亡）时不进建筑师，改播结局 CG，
/// 之后照样调 WinRun() 走成功结算。未命中则放行，正常换幕 / 见建筑师。
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
internal static class SpecialEndingPatch
{
	static bool Prefix(ref Task __result)
	{
		if (!SpecialEndingState.TryConsumeForEnding())
		{
			return true;
		}

		Godot.GD.Print("[SpecialEnding] 宾邦好结局 → 跳过建筑师，播放结局 CG");

		// EnterNextAct 是 async Task 方法：Harmony 跳过它等于跳过编译器生成的状态机存根，
		// 返回值是 default(Task) = null，调用方 TaskHelper.RunSafely(null) 会抛 NRE。
		// 必须显式给一个已完成的 Task。
		__result = Task.CompletedTask;

		// 此刻仍在 NRewardsScreen 的点击回调栈里
		// （OnProceedButtonPressed → ActChangeSynchronizer → MoveToNextAct → 本方法），
		// 同步清覆盖层会把正在跑回调的奖励屏 free 掉，所以延迟到本帧末再启动流程。
		// 注意 lambda 必须没有返回值：Callable.From 会把返回类型转成 Godot Variant，
		// 直接 => TaskHelper.RunSafely(...) 会返回 Task 而抛 "not supported for conversion"。
		Godot.Callable.From(() =>
		{
			TaskHelper.RunSafely(SpecialEndingState.RunAsync());
		}).CallDeferred();
		return false;
	}
}

/// <summary>
/// 真结局横幅：特殊结局的结算屏把原版硬编码的 BANNER.falseWin（"胜利……？"）
/// 换成本体本地化里预留、但从未被任何代码引用的 BANNER.trueWin（"胜利"）。
///
/// 同时在这里清空 SpecialEndingState —— 结算屏每次出现都意味着本局结束，
/// 顺带把标记清干净，避免泄漏到下一局（例如宾邦自爆后玩家反而团灭的失败局）。
/// </summary>
[HarmonyPatch(typeof(NGameOverScreen), "InitializeBannerAndQuote")]
internal static class TrueEndingBannerPatch
{
	static void Postfix(NGameOverScreen __instance)
	{
		bool specialEnding = SpecialEndingState.Triggered;
		SpecialEndingState.Reset();

		if (!specialEnding)
		{
			return;
		}

		NCommonBanner? banner = __instance.GetNodeOrNull<NCommonBanner>("%Banner");
		if (banner == null)
		{
			Godot.GD.PushWarning("[SpecialEnding] 找不到 %Banner，真结局横幅未替换");
			return;
		}

		banner.label.SetTextAutoSize(new LocString("game_over_screen", "BANNER.trueWin").GetRawText());
		Godot.GD.Print("[SpecialEnding] 结算横幅已替换为真结局（BANNER.trueWin）");
	}
}
