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

		// 额外抽 Amount 张牌：施加时 amount = 2（一张自助餐 +2 张），堆叠时每层 +2 张。
		// 注意这里**不能**再乘 2 —— 原来写成 Amount × 2 是重复乘了一次：
		// 卡面文案写"额外抽 2 张"，实际却抽了 4 张（堆叠时翻倍得更离谱）。
		await CardPileCmd.Draw(choiceContext, (decimal)Amount, Owner.Player);
	}

	/// <summary>
	/// 自己的回合开始时重置标记。
	/// 用 BeforeSideTurnStart 而不是 AfterSideTurnStart：回合开始抽 5 张发生在 SetupPlayerTurn
	/// （CombatManager 里先于 AfterSideTurnStart 调用），用 AfterSideTurnStart 重置会漏掉
	/// 起手抽到的食物牌（上一回合触发过的话，flag 到起手抽牌时还是 true）。
	/// 这个钩子按基类文档在"能量重置 / 抽牌之前"触发，正好覆盖起手那次抽牌。
	/// </summary>
	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(Owner))
		{
			GetInternalData<Data>().triggeredThisTurn = false;
		}
		return Task.CompletedTask;
	}
}
