using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山杖：每次切换场景时获得 3 点防御。
/// 通过监听 MyClimbing 的场景计数器变化来触发。
/// 商店稀有度。
/// </summary>
public sealed class Alpenstock : RelicModel
{
	private int _lastKnownSceneValue = -1;

	public override RelicRarity Rarity => RelicRarity.Shop;

	public override async Task BeforeCombatStart()
	{
		_lastKnownSceneValue = GetSceneValue();
	}

	public override async Task AfterSideTurnStart(
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (!participants.Contains(base.Owner?.Creature))
		{
			return;
		}

		int currentScene = GetSceneValue();

		// 场景发生变化时获得 3 点防御
		if (_lastKnownSceneValue >= 0 && currentScene != _lastKnownSceneValue && base.Owner?.Creature != null)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner.Creature, 3m, ValueProp.Move, null);
		}

		_lastKnownSceneValue = currentScene;
	}

	/// <summary>
	/// 获取当前 MyClimbing（或其子类）的场景值。
	/// 如果玩家没有该遗物则返回 -1。
	/// </summary>
	private int GetSceneValue()
	{
		MyClimbing? climbing = base.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
		return climbing?.DisplayAmount ?? -1;
	}
}
