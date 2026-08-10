using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Powers;

/// <summary>
/// 秘制辣酱：每当你打出一张食物牌，获得 6 点炎热值。
/// </summary>
public sealed class SecretRecipeChiliSaucePower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每打出一张食物牌获得的炎热值。
	/// </summary>
	public int HeatPerFood => Amount;

	/// <summary>
	/// 每打出一张食物牌时，获得 6 点炎热值。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只对食物牌生效（IFoodCard 是 mod 自定义的接口）
		if (cardPlay.Card is not IFoodCard)
		{
			return;
		}

		Flash(); // 秘制辣酱图标闪烁，提示玩家触发了效果

		// 获得炎热值
		await PowerCmd.Apply<HeatPower>(choiceContext, Owner, HeatPerFood, Owner, null);
	}
}
