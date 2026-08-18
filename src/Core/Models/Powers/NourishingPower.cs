using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 滋补：每当你打出一张食物牌，获得 1 层覆甲，1 点力量。
/// </summary>
public sealed class NourishingPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每打出一张食物牌获得的覆甲层数。
	/// </summary>
	public int PlatingPerFood => Amount;

	/// <summary>
	/// 每打出一张食物牌时，获得 1 层覆甲和 1 点力量。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只对打出者是自己时生效（多人模式：其他玩家打出的牌不触发本能力）
		if (cardPlay.Card.Owner != Owner.Player)
		{
			return;
		}

		// 只对食物牌生效（IFoodCard 是 mod 自定义的接口）
		if (cardPlay.Card is not IFoodCard)
		{
			return;
		}

		Flash(); // 滋补图标闪烁，提示玩家触发了效果

		// 1. 获得 1 层覆甲
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, PlatingPerFood, Owner, null);

		// 2. 获得 1 点力量
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, PlatingPerFood, Owner, null);
	}
}
