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
			ModelDb.Relic<MyClimbing>() // 注册您刚刚写好的初始遗物
			
			// 示例占位（当您以后设计了新遗物时，可以解开注释并在此处添加）：
			// ModelDb.Relic<ScoutBadge>(),
			// ModelDb.Relic<CampingTent>()
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
