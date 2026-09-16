using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的荣耀 — 好结局奖励遗物，也是隐藏 Boss 的入场券。
/// 获得方式：在第4幕Boss战中希望层数达标后获得（HopePower）。
/// 效果：NadirActMap.SecondBossMapPoint 依赖本遗物才返回"第二个 Boss（隐藏 Boss）"节点；
/// 这里在获得后重渲一次地图，让该节点真正出现在第四层地图上。
/// </summary>
public sealed class ScoutGlory : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;
	public override bool IsAllowedInShops => false;
	protected override string IconBaseName => "scout_glory";
	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		// 好结局：让第四层地图上出现隐藏 Boss（第二个 Boss）节点。
		// NadirActMap.SecondBossMapPoint 在本遗物加入玩家遗物列表后才返回非 null
		// （RelicCmd 先 AddRelicInternal 再调 AfterObtained，顺序安全）。
		// NMapScreen.SetMap 是原版运行中重建地图的正规入口（MapCmd 亦如此）。
		if (base.Owner?.RunState is RunState runState && runState.Map != null)
		{
			NMapScreen.Instance?.SetMap(runState.Map, runState.Rng.Seed, clearDrawings: false);
			GD.Print("[ScoutGlory] 好结局达成，隐藏 Boss 节点已加入第四层地图。");
		}

		await Task.CompletedTask;
	}
}
