using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 小烤：烤1张手牌，对所有敌人造成3点伤害2（3）次。
/// 烤：如果是食物卡，将其升级；如果是其他卡，将其消耗。
/// 1 费，攻击牌，普通稀有度，目标所有敌人。
/// </summary>
public sealed class SmallRoast : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：small_roast.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/small_roast.png");

	// 基础变量：3 点伤害，攻击 2 次（升级后 3 次）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(3m, ValueProp.Move),
		new RepeatVar(2)
	};

	public SmallRoast()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 烤1张手牌：食物卡升级，其他卡消耗
		CardModel? selected = (await CardSelectCmd.FromHand(
			context: choiceContext,
			player: base.Owner,
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
			filter: null,
			source: this)).FirstOrDefault();

		if (selected != null)
		{
			// 记录"被烤"状态（供大炮等卡牌打出时判断）
			RoastTracker.MarkRoasted(selected);

			if (selected is Cannon)
			{
				// 大炮被烤 -> 自动打出：由大炮 OnPlay 判断被烤状态，
				// 造成 aoe24 伤害并自消耗
				await CardCmd.AutoPlay(choiceContext, selected, null);
			}
			else if (selected is IFoodCard)
			{
				// 其他食物卡 -> 升级
				CardCmd.Upgrade(selected);
			}
			else
			{
				// 其他卡 -> 消耗
				await CardCmd.Exhaust(choiceContext, selected);
			}
		}

		// 2. 对所有敌人造成 3 点伤害 2（3）次
		int hitCount = base.IsUpgraded ? 3 : 2;
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount(hitCount)
			.FromCard(this, cardPlay)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		// 升级后攻击次数 2 -> 3
		base.DynamicVars["Repeat"].UpgradeValueBy(1m);
	}
}
