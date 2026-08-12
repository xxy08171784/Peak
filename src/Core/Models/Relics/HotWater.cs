using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 热水：在战斗开始时，获得 25 炎热值。
/// 罕见稀有度。
/// </summary>
public sealed class HotWater : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom && base.Owner?.Creature != null)
		{
			Flash();
			await PowerCmd.Apply<HeatPower>(
				new ThrowingPlayerChoiceContext(),
				base.Owner.Creature,
				25m,
				base.Owner.Creature,
				null
			);
		}
	}
}
