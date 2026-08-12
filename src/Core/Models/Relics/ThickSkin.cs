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
/// 皮糙肉厚：当你被给与一种 debuff 时，获得 5 点防御。
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
		// 只处理：owner 被施加 debuff 且层数增加
		if (power.Owner != base.Owner?.Creature || amount <= 0 || power.Type != PowerType.Debuff)
		{
			return;
		}

		Flash();
		await CreatureCmd.GainBlock(base.Owner!.Creature, 5m, ValueProp.Move, null);
	}
}
