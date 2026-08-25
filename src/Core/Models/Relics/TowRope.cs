using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 牵引绳：每打出三张技能牌就抽一张牌。
/// 稀有稀有度。
/// </summary>
public sealed class TowRope : RelicModel
{
	private int _skillCount;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter => CombatManager.Instance?.IsInProgress ?? false;
	public override int DisplayAmount => _skillCount % 3;

	public override Task BeforeCombatStart()
	{
		_skillCount = 0;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner || cardPlay.Card.Type != CardType.Skill)
		{
			return;
		}

		_skillCount++;
		InvokeDisplayAmountChanged();

		if (_skillCount % 3 == 0)
		{
			Flash();
			await CardPileCmd.Draw(choiceContext, 1m, base.Owner);
		}
	}
}
