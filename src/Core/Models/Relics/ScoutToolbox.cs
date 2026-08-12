using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using peak.Core.Models.Cards;

namespace peak.Core.Models.Relics;

/// <summary>
/// 童子军工具箱：在每场战斗中第一次打出道具牌时抽两张牌。
/// 罕见稀有度。（为避免与主游戏 Toolbox 冲突，类名使用 ScoutToolbox）
/// </summary>
public sealed class ScoutToolbox : RelicModel
{
	private bool _usedThisCombat;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is CombatRoom)
		{
			_usedThisCombat = false;
			base.Status = RelicStatus.Active;
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_usedThisCombat)
		{
			return;
		}

		if (cardPlay.Card.Owner == base.Owner && cardPlay.Card is IItemCard)
		{
			_usedThisCombat = true;
			base.Status = RelicStatus.Normal;
			Flash();
			await CardPileCmd.Draw(choiceContext, 2m, base.Owner);
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		_usedThisCombat = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
