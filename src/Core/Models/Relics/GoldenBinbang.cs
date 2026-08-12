using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace peak.Core.Models.Relics;

/// <summary>
/// 黄金宾邦：当你死亡时，回复 3 点生命，获得 2 层无实体，然后消耗（标记为已用完）。
/// 稀有稀有度。
/// 使用 LizardTail 同款的 ShouldDieLate + AfterPreventingDeath 模式。
/// </summary>
public sealed class GoldenBinbang : RelicModel
{
	private bool _wasUsed;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsUsedUp => _wasUsed;

	[SavedProperty]
	public bool WasUsed
	{
		get => _wasUsed;
		set
		{
			AssertMutable();
			_wasUsed = value;
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != base.Owner?.Creature)
		{
			return true;
		}
		if (WasUsed)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		Flash();
		WasUsed = true;

		// 回复 3 点生命
		await CreatureCmd.Heal(creature, 3m);

		// 获得 2 层无实体
		await PowerCmd.Apply<IntangiblePower>(
			new ThrowingPlayerChoiceContext(),
			creature,
			2m,
			creature,
			null
		);
	}
}
