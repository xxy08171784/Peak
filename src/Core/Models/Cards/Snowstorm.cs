using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

/// <summary>
/// 暴风雪：切换到雪山场景（失去至多 10 层炎热，给予所有敌人 1 层寒冷），
/// 额外失去 20（25）点炎热，给予所有敌人 1（2）层寒冷。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Snowstorm : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/snowstorm.png");

	// 基础变量：额外失去 20 点炎热（升级后 25），额外给予 1 层寒冷（升级后 2）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("HeatLoss", 20m),
		new PowerVar<ColdPower>(1m)
	};

	public Snowstorm()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 切换到雪山场景（场景 3），触发场景效果（失去至多 10 炎热 + 全体 1 层寒冷）
		MyClimbing? myClimbing = base.Owner.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing != null)
		{
			int currentScene = myClimbing.DisplayAmount;
			int targetScene = 3; // 雪山
			int delta = (targetScene - currentScene + 4) % 4;
			if (delta != 0)
			{
				await myClimbing.ModifyEnvironmentValue(choiceContext, delta);
			}
			else
			{
				// 已在雪山场景，直接触发场景效果
				await myClimbing.ExecuteStateEffect(choiceContext, targetScene);
			}
		}

		// 2. 额外失去 20（25）点炎热
		decimal heatLoss = base.DynamicVars["HeatLoss"].BaseValue;
		HeatPower? heatPower = base.Owner.Creature.GetPower<HeatPower>();
		if (heatPower != null && heatPower.Amount > 0)
		{
			int reduceAmount = (int)System.Math.Min(heatPower.Amount, heatLoss);
			await PowerCmd.ModifyAmount(choiceContext, heatPower, -reduceAmount, base.Owner.Creature, this);
		}

		// 3. 额外给予所有敌人 1（2）层寒冷
		decimal coldStacks = base.DynamicVars["ColdPower"].BaseValue;
		foreach (var enemy in base.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<ColdPower>(
				choiceContext,
				enemy,
				coldStacks,
				base.Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后失去炎热 20 -> 25 (+5)，寒冷层数 1 -> 2 (+1)
		base.DynamicVars["HeatLoss"].UpgradeValueBy(5m);
		base.DynamicVars["ColdPower"].UpgradeValueBy(1m);
	}
}
