using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 自助餐：每回合第一次抽到食物卡时，额外抽 2 张牌。
/// </summary>
public sealed class BuffetPower : PowerModel
{
	// 内部数据：记录本回合是否已触发过
	private class Data
	{
		public bool triggeredThisTurn;
	}

	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>初始化内部状态</summary>
	protected override object InitInternalData() => new Data();

	/// <summary>
	/// 每回合第一次抽到食物卡时，额外抽 2 张牌。
	/// </summary>
	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		Data data = GetInternalData<Data>();

		// 本回合已触发过则不再触发
		if (data.triggeredThisTurn)
		{
			return;
		}

		// 只处理属于自己抽到的卡
		if (card.Owner != Owner.Player)
		{
			return;
		}

		// 只处理食物卡
		if (card is not IFoodCard)
		{
			return;
		}

		// 立即置位，防止额外抽的卡连锁触发
		data.triggeredThisTurn = true;

		Flash(); // 自助餐图标闪烁，提示玩家触发了效果

		// 额外抽 2 张牌
		await CardPileCmd.Draw(choiceContext, 2m, Owner.Player);
	}

	/// <summary>
	/// 自己的回合开始时重置标记（在回合开始抽牌之前触发，不会错过回合开始的抽牌）。
	/// </summary>
	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(Owner))
		{
			GetInternalData<Data>().triggeredThisTurn = false;
		}
		await Task.CompletedTask;
	}
}
