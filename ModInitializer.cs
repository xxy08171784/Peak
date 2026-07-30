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
using MegaCrit.Sts2.Core.Models.Relics;
using peak.Core.Models.Relics;
using peak.Core.Models.RelicsPools;

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

<<<<<<< HEAD
			// 2. 注册关联卡牌和遗物到对应的池子中（注意：这里只传递 typeof 结构，非常安全，不会触发任何实例化，因此绝不会导致闪退）
		//	ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(RockBoltCard));
			//ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(StrikeScout));
			//ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(DefendScout));
			//ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(MixedNuts));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(MyClimbing));

=======
			 //2. 注册关联卡牌和遗物到对应的池子中（注意：这里只传递 typeof 结构，非常安全，不会触发任何实例化，因此绝不会导致闪退）
			ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(RockBoltCard));
			ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(StrikeScout));
			ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(DefendScout));
			ModHelper.AddModelToPool(typeof(ScoutCardPool), typeof(MixedNuts));
			ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(MyClimbing));
>>>>>>> origin/xxy
			Log.Info("加载成功！");
		}
	}
}
