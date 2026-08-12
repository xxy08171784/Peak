using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 打火石：进入休息处时回复 6 点生命。
/// 普通稀有度。
/// </summary>
public sealed class Flintstone : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is RestSiteRoom && base.Owner?.Creature != null)
		{
			Flash();
			await CreatureCmd.Heal(base.Owner.Creature, 6m);
		}
	}
}
