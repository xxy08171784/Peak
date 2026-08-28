using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童军的野心 - 第3颗宝石遗物。
/// 第一次打败精英后获得。
/// 精英战开始时获得2点力量。
/// </summary>
public sealed class ScoutAmbition : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.None;
    public override bool IsAllowedInShops => false;
    protected override string IconBaseName => "scout_ambition";

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room.RoomType == RoomType.Elite && base.Owner?.Creature != null)
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(),
                base.Owner.Creature, 2m,
                base.Owner.Creature, null);
        }
    }
}