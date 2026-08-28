using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Powers;

/// <summary>
/// 游龙形态：每当你打出 1 张牌时，从以下效果中随机选择 1 个获得：
/// 6 层炎热、2 层孢子、1 层中毒、场景+1、抽 1 张牌、1 点费用、1 层再生、1 层覆甲。
/// </summary>
public sealed class WanderingDragonFormPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（控制了每张牌触发时的随机次数）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 每次玩家打出牌后，触发随机效果。
	/// </summary>
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 只在宿主玩家打出的牌触发（避免无限触发自身）
		if (cardPlay.Card.Owner.Creature != Owner)
		{
			return;
		}

		Flash(); // 图标闪烁提示

		int rolls = (int)Amount; // 层数 = 每次随机次数
		for (int i = 0; i < rolls; i++)
		{
			await RollRandomEffect(choiceContext);
		}
	}

	private async Task RollRandomEffect(PlayerChoiceContext choiceContext)
	{
		// 8 种随机效果
		int roll = Owner.Player!.RunState.Rng.CombatCardGeneration.NextInt(0, 8);
		switch (roll)
		{
			case 0: // 6 层炎热
				await PowerCmd.Apply<HeatPower>(choiceContext, Owner, 6m, Owner, null);
				break;
			case 1: // 2 层孢子
				await PowerCmd.Apply<SporePower>(choiceContext, Owner, 2m, Owner, null);
				break;
			case 2: // 1 层中毒
				await PowerCmd.Apply<ZhongduPower>(choiceContext, Owner, 1m, Owner, null);
				break;
			case 3: // 场景 +1
				MyClimbing? climbing = Owner.Player!.Relics.OfType<MyClimbing>().FirstOrDefault();
				if (climbing != null)
				{
					await climbing.ModifyEnvironmentValue(choiceContext, 1);
				}
				break;
			case 4: // 抽 1 张牌
				await CardPileCmd.Draw(choiceContext, 1m, Owner.Player!);
				break;
			case 5: // 1 点费用
				await PlayerCmd.GainEnergy(1m, Owner.Player!);
				break;
			case 6: // 1 层再生
				await PowerCmd.Apply<RegenPower>(choiceContext, Owner, 1m, Owner, null);
				break;
			case 7: // 1 层覆甲
				await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, 1m, Owner, null);
				break;
		}
	}
}