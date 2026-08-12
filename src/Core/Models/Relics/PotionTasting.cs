using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 药水品鉴：每次获得药水时增加 1 生命值上限。
/// 稀有稀有度。
/// </summary>
public sealed class PotionTasting : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override async Task AfterPotionProcured(PotionModel potion)
	{
		if (base.Owner?.Creature != null)
		{
			Flash();
			await CreatureCmd.GainMaxHp(base.Owner.Creature, 1m);
		}
	}
}
