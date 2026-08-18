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
using peak.Core.Models.Potions;
using peak.Core.Models.Relics;
using peak.Patches;

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

			// 1.5. 用反射将 Scout epoch 注入 EpochModel._allEpochs（无法通过 Harmony getter 补丁实现）
			ScoutEpochRegistrar.Register();

			// 2. 注册关联卡牌和遗物到对应的池子中
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
			// 混池药水
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(PoisonedBottle));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(CactusJuice));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(SleepingPill));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(MagicBeanJuice));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(SpritePotion));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(BottledMist));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(BottledTornado));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(MilkPotion));
			ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(FreezeBottle));		// 多人混池卡牌（无色池，仅多人模式出现）
		ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(PrankTeammate));
		ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(LowerRope));
		ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(CursedSkull));
		ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(FriendshipHorn));			Log.Info("加载成功！");
		}
	}
}
