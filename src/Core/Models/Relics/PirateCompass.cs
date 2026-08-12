using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 海盗罗盘：打开宝箱时额外获得 75 金币。
/// 进入宝箱房时触发。
/// 罕见稀有度。
/// </summary>
public sealed class PirateCompass : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is TreasureRoom && base.Owner != null)
		{
			Flash();
			await PlayerCmd.GainGold(75m, base.Owner);
		}
	}
}
