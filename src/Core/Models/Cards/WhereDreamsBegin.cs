using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;
using System.Linq;

namespace peak.Core.Models.Cards;

/// <summary>
/// 梦开始的地方：切换到 0 海岛。
/// 0 费，技能牌，普通稀有度，目标自身，消耗，升级后获得保留。
/// </summary>
public sealed class WhereDreamsBegin : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/where_dreams_begin.png");

	// 消耗关键词（升级前后都有）
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public WhereDreamsBegin()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		MyClimbing? myClimbing = base.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing != null)
		{
			// 切换到 0 海岛
			await myClimbing.SetEnvironmentValue(choiceContext, 0);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后获得保留
		AddKeyword(CardKeyword.Retain);
	}
}
