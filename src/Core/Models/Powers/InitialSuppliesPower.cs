using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Cards;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 初始物资：每当环境切换到 0 海岛时，获得一张棉花糖。
/// 通过 IEnvironmentAware 接口（替代旧的静态事件+async void 模式）。
/// </summary>
public sealed class InitialSuppliesPower : PowerModel, IEnvironmentAware
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 由 MyClimbing 在环境切换时调用（替代旧的 async void 事件处理器）。
	/// </summary>
	public async Task OnEnvironmentChanged(PlayerChoiceContext choiceContext, Player player, int previousValue, int currentValue)
	{
		// 只响应自己所属的玩家；且必须切换到 0 海岛
		if (player != Owner.Player || Owner.IsDead || currentValue != 0)
		{
			return;
		}

		if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance?.IsInProgress != true)
		{
			return;
		}

		Flash();

		// 获得 Amount 张棉花糖（每层 1 张，打多张可叠加）
		for (int i = 0; i < Amount; i++)
		{
			Marshmallow marshmallow = Owner.CombatState.CreateCard<Marshmallow>(player);
			await CardPileCmd.AddGeneratedCardsToCombat(new[] { marshmallow }, PileType.Hand, player);
		}
	}
}
