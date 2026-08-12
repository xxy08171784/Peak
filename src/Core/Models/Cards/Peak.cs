using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

/// <summary>
/// PEAK：如果你在本回合内按"01230"顺序切换过场景，获得本场战斗胜利。
/// 2 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class Peak : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/peak.png");

	// 保留关键字
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	// 悬停提示：显示保留关键字
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromKeyword(CardKeyword.Retain)
	};

	public Peak()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner == null)
		{
			return;
		}

		// 从遗物上读取本回合的切换序列，判定是否出现过 01230
		MyClimbing climbingRelic = base.Owner.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (climbingRelic != null && climbingRelic.HasSeen01230SequenceThisTurn)
		{
			// 条件满足：杀死所有活着的敌人
			List<Creature> aliveEnemies = base.CombatState.Enemies.Where(e => e.IsAlive).ToList();
			if (aliveEnemies.Count > 0)
			{
				await CreatureCmd.Kill(aliveEnemies);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}