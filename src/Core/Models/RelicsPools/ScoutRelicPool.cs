using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using peak.Core.Models.Relics;

namespace peak.Core.Models.RelicsPools;

public sealed class ScoutRelicPool : RelicPoolModel
{
	// 对应童子军的能量颜色名称
	public override string EnergyColorName => "scout";

	// 科学馆/图鉴界面中遗物的描边颜色（使用金色/黄色）
	public override Color LabOutlineColor => new Color("FFD700");

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		// 在这里注册属于童子军的所有专属遗物
		return new RelicModel[]
		{
			// 初始遗物
			ModelDb.Relic<MyClimbing>(),
			ModelDb.Relic<MountaineeringExpert>(),
			// 角色专属遗物
			ModelDb.Relic<Match>(),
			ModelDb.Relic<HotWater>(),
			ModelDb.Relic<ScoutToolbox>(),
			ModelDb.Relic<PortableRations>(),
			ModelDb.Relic<NutritionPyramid>(),
			ModelDb.Relic<ColdStorage>(),
			// 商店角色遗物
			ModelDb.Relic<Alpenstock>(),
			// 第4幕宝石遗物（不参与正常奖励，但需注册到池中才能在图鉴/详细介绍中显示）
			ModelDb.Relic<ScoutHospitality>(),
			ModelDb.Relic<ScoutPerseverance>(),
			ModelDb.Relic<ScoutAmbition>(),
			ModelDb.Relic<ScoutEnterprise>(),
			// 好结局奖励遗物
			ModelDb.Relic<ScoutGlory>(),
		};
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();

		// 如果您的 Mod 计划使用“时代”（Epoch）系统来逐步解锁遗物，可以参考以下格式解除注释：
		/*
		if (!unlockState.IsEpochRevealed<Scout3Epoch>())
		{
			list.RemoveAll((RelicModel r) => Scout3Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		*/

		return list;
	}
}
