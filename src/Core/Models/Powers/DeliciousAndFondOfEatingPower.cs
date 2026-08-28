using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 好吃爱吃继续吃：每当你打出一张食物牌，抽 1（2）张牌。
/// </summary>
public sealed class DeliciousAndFondOfEatingPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每打出一张食物牌抽的牌数。
	/// </summary>
	public int CardsPerFood => Amount;

	/// <summary>
	/// 每打出一张食物牌时，抽 1（2）张牌。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只对打出者是自己或卡牌目标是自己时生效
		//（多人模式：其他玩家打出的牌不触发；但善意投喂等指向自己的卡牌会触发）
		if (cardPlay.Card.Owner != Owner.Player && cardPlay.Target != Owner)
		{
			return;
		}

		// 只对食物牌生效（IFoodCard 是 mod 自定义的接口）
		if (cardPlay.Card is not IFoodCard)
		{
			return;
		}

		Flash(); // 好吃爱吃继续吃图标闪烁，提示玩家触发了效果

		// 抽 1（2）张牌
		await CardPileCmd.Draw(choiceContext, CardsPerFood, Owner.Player);
	}
}
