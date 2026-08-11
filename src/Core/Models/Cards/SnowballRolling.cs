using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 滚雪球：若雪球不存在则生成一张雪球加入手牌，若已存在则将其所有数值 +1 并移回手牌。
/// 打出后滚雪球本身回到手牌底部。
/// 1 费，技能牌，罕见稀有度，目标自身。升级后获得固有。
/// </summary>
public sealed class SnowballRolling : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/snowball_rolling.png");
	public SnowballRolling()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 搜索雪球
		Snowball? existingSnowball = base.Owner.PlayerCombatState.AllCards
			.OfType<Snowball>()
			.FirstOrDefault();

		if (existingSnowball != null)
		{
			existingSnowball.AddAllValues(1m);
			if (existingSnowball.Pile?.Type != PileType.Hand)
			{
				await CardPileCmd.Add(existingSnowball, PileType.Hand);
			}
		}
		else
		{
			Snowball snowball = base.CombatState.CreateCard<Snowball>(base.Owner);
			await CardPileCmd.AddGeneratedCardsToCombat(new[] { snowball }, PileType.Hand, base.Owner);
		}

		// 🌟 关键：在 OnPlay 结束前，直接将自己移回手牌底部！
		// 这样游戏后续就不会再把这张牌丢进弃牌堆了。
		await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom);
	}

	protected override void OnUpgrade()
	{
		// 升级后获得固有（每场战斗开局必定在手牌中）
		AddKeyword(CardKeyword.Innate);
	}
}
