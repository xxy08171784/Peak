using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace peak.Patches;

/// <summary>
/// 修复时间线缩略图显示 "nope"（missing_power）的问题。
///
/// 原因：时间线槽位（NEpochSlot）加载 EpochModel.Portrait，
/// 即图集路径 res://images/atlases/epoch_atlas.sprites/scoutX_epoch.tres，
/// 但 scout 的 epoch 没有被打包进游戏的 epoch_atlas.tpsheet，
/// 而 AtlasResourceLoader.GetFallbackPath 的 switch 里没有 epoch_atlas 分支，
/// 于是回退到 missing_power.png。
///
/// 详情大图（NEpochInspectScreen）使用的是 RealPortrait，
/// 会回退到 res://images/timeline/epoch_portraits/placeholder/scoutX_epoch.png，所以正常。
///
/// 修复：给 GetFallbackPath 增加 epoch_atlas 分支，让缺失的 scout sprite
/// 回退到时间线占位肖像图，与详情大图保持一致。
/// </summary>
[HarmonyPatch(typeof(AtlasResourceLoader), "GetFallbackPath")]
public static class EpochAtlasFallbackPatch
{
	static void Postfix(string atlasName, string spriteName, ref string? __result)
	{
		if (atlasName != "epoch_atlas" || __result != null)
		{
			return;
		}
		// 与 EpochModel.PlaceholderPortraitPath 保持一致：
		// res://images/timeline/epoch_portraits/placeholder/{id}.png
		__result = "res://images/timeline/epoch_portraits/placeholder/" + spriteName + ".png";
	}
}
