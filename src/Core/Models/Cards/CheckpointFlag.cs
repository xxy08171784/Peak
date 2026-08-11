using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 检查点旗帜：将你的生命值回溯到你上回合结束时的值。
/// 2 费，技能牌，稀有稀有度，目标自身，消耗，保留。
/// </summary>
public sealed class CheckpointFlag : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：checkpoint_flag.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/checkpoint_flag.png");

	// 消耗 + 保留关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, CardKeyword.Retain };

	public CheckpointFlag()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 确保检查点追踪器已生效（首次打出时给自己附加）
		if (base.Owner.Creature.GetPower<CheckpointTrackerPower>() == null)
		{
			await PowerCmd.Apply<CheckpointTrackerPower>(
				choiceContext,
				base.Owner.Creature,
				1m,
				base.Owner.Creature,
				this
			);
		}

		// 2. 回溯生命值到上回合结束时的值
		int targetHp = CheckpointFlagTracker.GetLastTurnEndHp(base.Owner);
		int currentHp = base.Owner.Creature.CurrentHp;

		// 只在实际值不同时更新（防止无效动画）
		if (targetHp != currentHp)
		{
			await CreatureCmd.SetCurrentHp(base.Owner.Creature, targetHp);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}
