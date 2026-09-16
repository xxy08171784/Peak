using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using peak.Core.Models.Monsters;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的荣耀 — 好结局奖励遗物，也是隐藏 Boss 的入场券。
/// 获得方式：在第4幕Boss战中希望层数达标后获得（HopePower）。
///
/// 效果 1：NadirActMap.SecondBossMapPoint 依赖本遗物才返回"第二个 Boss（隐藏 Boss）"节点；
///         这里在获得后重渲一次地图，让该节点真正出现在第四层地图上。
/// 效果 2：宾邦「销毁」自爆时免于死亡并回复 50% 最大生命，之后消耗（一次性）。
///         复活写法照抄原版蜥蜴尾巴 LizardTail：ShouldDieLate + AfterPreventingDeath + WasUsed。
/// </summary>
public sealed class ScoutGlory : RelicModel
{
	private bool _wasUsed;

	public override RelicRarity Rarity => RelicRarity.Rare;
	public override bool IsAllowedInShops => false;
	protected override string IconBaseName => "scout_glory";
	public override bool HasUponPickupEffect => true;

	public override bool IsUsedUp => _wasUsed;

	/// <summary>回血量：最大生命的百分比（与蜥蜴尾巴同款）。</summary>
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new HealVar(50m)
	};

	[SavedProperty]
	public bool WasUsed
	{
		get => _wasUsed;
		set
		{
			AssertMutable();
			_wasUsed = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

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

	/// <summary>
	/// 只挡下「宾邦正在自爆」时的那一次死亡；其余情况一律返回 true（不干预、也不消耗本遗物）。
	///
	/// 用 ShouldDieLate 而不是 ShouldDie：同原版蜥蜴尾巴，走"最后一道防线"那一趟，
	/// 让瓶中小精灵这类更早的免死效果优先。
	/// 注意引擎一次死亡只会选中**第一个**说"不"的 preventer（Hook.ShouldDie 遇 false 即返回），
	/// 所以与黄金宾邦等其它免死遗物同时持有且都未消耗时，只会有一个生效，另一个保留。
	/// </summary>
	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != base.Owner?.Creature)
		{
			return true;
		}

		if (WasUsed)
		{
			return true;
		}

		if (!IsBinbangSelfDestructing(creature))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// 挡下死亡后回血。**必须把血回到 0 以上**：
	/// CreatureCmd 在死亡被挡下后会检查 <c>if (creature.IsDead) Kill(..., recursion + 1)</c>，
	/// 血量没上去就会往复递归，10 次后抛 "something is continually preventing the last
	/// creature from being killed!"。蜥蜴尾巴的 <c>Math.Max(1m, ...)</c> 也是这个原因。
	/// </summary>
	public override async Task AfterPreventingDeath(Creature creature)
	{
		Flash();
		WasUsed = true;

		decimal amount = Math.Max(1m, (decimal)creature.MaxHp * (base.DynamicVars.Heal.BaseValue / 100m));
		await CreatureCmd.Heal(creature, amount);
	}

	/// <summary>场上是否有正在自爆的宾邦。从死亡玩家所在的战斗状态里找，不用静态状态。</summary>
	private static bool IsBinbangSelfDestructing(Creature creature)
		=> creature.CombatState?.Enemies.Any(e => e.Monster is Binbang { IsSelfDestructing: true }) == true;
}
