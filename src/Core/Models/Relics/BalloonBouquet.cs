using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 气球束：拾取时增加 15 生命上限。
/// 稀有稀有度。
/// </summary>
public sealed class BalloonBouquet : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		if (base.Owner?.Creature != null)
		{
			await CreatureCmd.GainMaxHp(base.Owner.Creature, 15m);
		}
	}
}
