using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Powers;

/// <summary>
/// 历练：每当你失去一次负面状态时，获得力量。
/// 按次数触发：一次失去多层也只算一次（例如散热一次失去 7 层炎热，只算 1 次）。
/// Amount 即每次触发获得的力量值，多张历练可叠加。
/// </summary>
public sealed class ExperienceAndTougheningPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（层数 = 每次触发获得的力量值）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每次失去负面状态时获得的力量值。
	/// </summary>
	public int StrengthPerTrigger => Amount;

	/// <summary>
	/// 监听全局 power 变化：当自己身上的 debuff 层数减少（失去 debuff）时，获得力量。
	/// 按事件次数触发：无论一次失去多少层，都只算 1 次。
	/// </summary>
	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		// 只触发：层数减少 + 该 power 是 debuff + 在自己身上 + 非临时性
		if (amount < 0
			&& power.Type == PowerType.Debuff
			&& power.Owner == base.Owner
			&& power is not ITemporaryPower)
		{
			Flash(); // 历练图标闪烁

			await PowerCmd.Apply<StrengthPower>(
				choiceContext,
				base.Owner,
				StrengthPerTrigger,
				base.Owner,
				null
			);
		}
	}
}
