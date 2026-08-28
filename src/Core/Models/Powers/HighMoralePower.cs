using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 士气高涨：每当玩家切换环境时，获得 1 点覆甲并抽 1 张牌。
/// 通过 IEnvironmentAware 接口（替代旧的静态事件+async void 模式）。
/// </summary>
public sealed class HighMoralePower : PowerModel, IEnvironmentAware
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	// 每次切换环境获得的覆甲（随层数叠加，层数即 Amount）
	public int BlockPerTrigger => Amount;

	// 每次切换环境抽的牌数（随层数叠加，层数即 Amount）
	public int CardsPerTrigger => Amount;

	/// <summary>
	/// 由 MyClimbing 在环境切换时调用。
	/// 场景对各玩家独立，因此 choiceContext 始终属于 Power 持有者自己。
	/// </summary>
	public async Task OnEnvironmentChanged(PlayerChoiceContext choiceContext, Player player, int previousValue, int currentValue)
	{
		// 只响应自己所属的玩家
		if (player != Owner.Player || Owner.IsDead)
		{
			return;
		}
		if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance?.IsInProgress != true)
		{
			return;
		}

		Flash();

		// 1. 获得覆甲
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, BlockPerTrigger, Owner, null);

		// 2. 抽牌
		if (CardsPerTrigger > 0)
		{
			await CardPileCmd.Draw(choiceContext, CardsPerTrigger, player);
		}
	}
}
