using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 毒刺：当被敌人攻击命中时，反击该名敌人 3（4）点中毒（原版中毒 PoisonPower）。
/// </summary>
public sealed class TelsonPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 被攻击命中时：给攻击者施加等同层数的中毒。
	/// </summary>
	public override async Task AfterDamageReceived(
		PlayerChoiceContext choiceContext,
		Creature target,
		DamageResult _,
		ValueProp props,
		Creature? dealer,
		CardModel? __)
	{
		// 只处理自己受到的攻击伤害
		if (target != Owner || dealer == null || !props.IsPoweredAttack())
		{
			return;
		}

		Flash(); // 毒刺图标闪烁，提示触发了效果

		// 给攻击者施加 3（4）点原版中毒
		await PowerCmd.Apply<PoisonPower>(
			choiceContext,
			dealer,
			Amount,
			Owner,
			null
		);
	}
}
