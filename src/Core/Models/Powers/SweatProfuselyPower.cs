using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

/// <summary>
/// 挥汗如雨：每打出一张攻击牌，失去 5（7）点炎热。
/// 失去炎热值会触发 HeatPower 对敌人造成伤害。
/// </summary>
public sealed class SweatProfuselyPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每打出一张攻击牌失去的炎热值层数。
	/// </summary>
	public int HeatLossPerAttack => Amount;

	/// <summary>
	/// 每打出一张攻击牌时，失去等同于层数的炎热值。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只对打出者是自己时生效（多人模式：其他玩家打出的牌不触发本能力）
		if (cardPlay.Card.Owner != Owner.Player)
		{
			return;
		}

		// 只对攻击牌生效
		if (cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}

		// 若当前没有炎热值则无事可做
		HeatPower? heatPower = Owner.GetPower<HeatPower>();
		if (heatPower == null || heatPower.Amount <= 0)
		{
			return;
		}

		Flash(); // 挥汗如雨图标闪烁，提示玩家触发了效果

		// 失去炎热值（不可为负）
		int toLose = System.Math.Min(heatPower.Amount, HeatLossPerAttack);
		await PowerCmd.ModifyAmount(choiceContext, heatPower, -toLose, Owner, null);
	}
}
