using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山杖：每次切换场景时获得 3 点防御。
/// 通过订阅 MyClimbing.SceneChanged 事件来实时响应场景切换（包括 Climb 卡牌触发的切换）。
/// 商店稀有度。
/// </summary>
public sealed class Alpenstock : RelicModel
{
	private MyClimbing? _climbingRelic;

	public override RelicRarity Rarity => RelicRarity.Shop;

	private void OnSceneChanged(int newScene)
	{
		if (base.Owner?.Creature == null || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}

		Flash();
		TaskHelper.RunSafely(
			CreatureCmd.GainBlock(base.Owner.Creature, 3m, ValueProp.Move, null));
	}

	public override Task BeforeCombatStart()
	{
		// 每场战斗开始时重新绑定事件
		_climbingRelic = base.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (_climbingRelic != null)
		{
			_climbingRelic.SceneChanged += OnSceneChanged;
		}
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		// 战斗结束时解绑
		if (_climbingRelic != null)
		{
			_climbingRelic.SceneChanged -= OnSceneChanged;
			_climbingRelic = null;
		}
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
