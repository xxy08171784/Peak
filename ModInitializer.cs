using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using peak.Core.Models.CardPools;
using peak.Core.Models.Cards;

using peak.Core.Models.Characters;
using peak.Core.Models.Relics;

namespace peak
{
	[ModInitializer(nameof(Initialize))]
	public static class ModInitializer
	{
		public static void Initialize()
		{
			Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
			// 1. 初始化 Harmony（这会自动激活我们在 ScoutUnlockPatch 中写的所有安全重定向补丁）
			var harmony = new Harmony("xxy_peak");
			harmony.PatchAll();

			// 2. 注册关联卡牌和遗物到对应的池子中（注意：这里只传递 typeof 结构，非常安全，不会触发任何实例化，因此绝不会导致闪退）
			// 混池遗物 — 普通
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Flintstone));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Telescope));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Tick));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(StumblingBlock));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Cactus));
			// 混池遗物 — 罕见
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(PirateCompass));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(GlowStick));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Rivet));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(HeavyBackpack));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Inertia));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(Tumbleweed));
			// 混池遗物 — 稀有
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(InventoryRelic));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(ClimbingSuit));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(BalloonBouquet));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(TowRope));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(PotionTasting));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(ThickSkin));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(GoldenBinbang));
			Log.Info("加载成功！");
		}
	}
}
