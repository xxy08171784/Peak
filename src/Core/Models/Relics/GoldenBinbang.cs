using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 黄金宾邦：当你死亡时，回复 3 点生命，获得 2 层无实体，然后消耗（标记为已用完）。
/// 稀有稀有度。
/// </summary>
public sealed class GoldenBinbang : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsUsedUp => _isUsed;

	private bool _isUsed;

	public override bool ShouldDie(Creature creature)
	{
		// 只保护持有者本人
		if (creature != base.Owner?.Creature || _isUsed)
		{
			return true;
		}

		// 阻止死亡，触发效果
		TaskHelper.RunSafely(TriggerSaveEffect());
		return false;
	}

	private async Task TriggerSaveEffect()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		Flash();

		var ctx = new ThrowingPlayerChoiceContext();

		// 回复 3 点生命
		await CreatureCmd.Heal(base.Owner.Creature, 3m);

		// 获得 2 层无实体
		await PowerCmd.Apply<IntangiblePower>(
			ctx,
			base.Owner.Creature,
			2m,
			base.Owner.Creature,
			null
		);

		// 标记为已用完
		_isUsed = true;
		InvokeDisplayAmountChanged();
	}
}
