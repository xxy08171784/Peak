using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Potions;

/// <summary>
/// 潘多拉药水：给予一名敌人随机负面效果。
/// 随机从：易伤(0-3)、虚弱(0-3)、减力量(0-3)、中毒(0-12)、寒冷(0-3) 中选择一种施加。
/// 稀有稀有度，角色专属，仅限战斗中使用，目标敌人。
/// </summary>
public sealed class PandoraPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;
	public override PotionUsage Usage => PotionUsage.CombatOnly;
	public override TargetType TargetType => TargetType.AnyEnemy;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		AssertValidForTargetedPotion(target);

		var rng = new Random();
		int effect = rng.Next(5);

		switch (effect)
		{
			case 0:
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target,
					rng.Next(4), base.Owner.Creature, null);
				break;
			case 1:
				await PowerCmd.Apply<WeakPower>(choiceContext, target,
					rng.Next(4), base.Owner.Creature, null);
				break;
			case 2:
				// 减力量：0 到 3 层负力量
				int strLoss = rng.Next(4);
				if (strLoss > 0)
				{
					await PowerCmd.Apply<StrengthPower>(choiceContext, target,
						-strLoss, base.Owner.Creature, null);
				}
				break;
			case 3:
				await PowerCmd.Apply<ZhongduPower>(choiceContext, target,
					rng.Next(13), base.Owner.Creature, null);
				break;
			case 4:
				await PowerCmd.Apply<ColdPower>(choiceContext, target,
					rng.Next(4), base.Owner.Creature, null);
				break;
		}
	}
}
