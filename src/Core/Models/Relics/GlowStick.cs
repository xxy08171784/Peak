using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 照明棒：进入 Boss 战斗时获得 3 点力量。
/// 罕见稀有度。
/// </summary>
public sealed class GlowStick : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room.RoomType == RoomType.Boss && base.Owner?.Creature != null)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(
				new ThrowingPlayerChoiceContext(),
				base.Owner.Creature,
				3m,
				base.Owner.Creature,
				null
			);
		}
	}
}
