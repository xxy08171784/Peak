using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

// ===== 手动补充导入游戏本体的药水和模型命名空间 =====
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using peak.Core.Models.Potions;

namespace peak.Core.Models.PotionPools;

public sealed class ScoutPotionPool : PotionPoolModel
{
	// 对应童子军的能量颜色名称
	public override string EnergyColorName => "scout";

	// 图鉴界面中药水瓶的描边颜色（使用黄色/金色）
	public override Color LabOutlineColor => new Color("FFD700");

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return new PotionModel[]
		{
			ModelDb.Potion<AloeJuice>(),
			ModelDb.Potion<SnowballPotion>(),
			ModelDb.Potion<PandoraPotion>()
		};
	}

	public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
	{
		// 目前未添加专属药水，直接返回全部（即空集合）。
		// 以后如果您加入了时代（Epoch）系统来限制药水解锁，可以仿照原版在这里加入判断：
		/*
		if (!unlockState.IsEpochRevealed<Scout4Epoch>())
		{
			return Array.Empty<PotionModel>();
		}
		*/
		return GenerateAllPotions();
	}
}
