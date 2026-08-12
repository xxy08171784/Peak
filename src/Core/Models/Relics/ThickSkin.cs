using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Relics;

/// <summary>
/// 皮糙肉厚：当你被给与一种新的 debuff 时（首次施加，不含层数叠加），获得 5 点格挡。
/// 稀有稀有度。
/// </summary>
public sealed class ThickSkin : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterPowerAmountChanged(
		PlayerChoiceContext choiceContext,
		PowerModel power,
		decimal amount,
		Creature? applier,
		CardModel? cardSource)
	{
		// 只处理：owner 本身被施加 debuff
		if (power.Owner != base.Owner?.Creature || power.Type != PowerType.Debuff)
		{
			return;
		}

		// amount == power.Amount 意味着这是首次施加（新创建的 power）
		// amount > 0 确保是获得而非失去
		if (amount > 0 && power.Amount == amount)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner!.Creature, 5m, ValueProp.Move, null);
		}
	}
}
