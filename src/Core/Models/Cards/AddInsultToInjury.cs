using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 雪上加霜：指定一名敌人，使其所有负面状态翻倍。
/// 2（1）费，技能牌，稀有稀有度，目标任意敌人。
/// </summary>
public sealed class AddInsultToInjury : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/add_insult_to_injury.png");

	public AddInsultToInjury()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 收集目标身上所有可翻倍的负面状态（先拷贝，避免遍历时修改集合）
		List<PowerModel> debuffs = cardPlay.Target.Powers
			.Where(p => p.Type == PowerType.Debuff
				&& p.Amount > 0
				&& p is not ITemporaryPower)
			.ToList();

		foreach (PowerModel debuff in debuffs)
		{
			int currentAmount = debuff.Amount;

			// 翻倍：增加等同于当前层数的量
			await PowerCmd.ModifyAmount(
				choiceContext,
				debuff,
				currentAmount,
				base.Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
