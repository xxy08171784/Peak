using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 抱团取暖：所有人获得 16（24）点炎热值。
/// 1 费，技能牌，罕见稀有度，目标全体友方。
/// 多人专属卡牌。
/// </summary>
public sealed class HuddleTogether : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 卡面尚未绘制，暂用 beta 占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	// 动态变量：获得炎热 16（升级后 24）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<HeatPower>("HeatPower", 16m)
	};

	public HuddleTogether()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal heat = base.DynamicVars["HeatPower"].BaseValue;

		// 所有玩家（自己 + 队友）获得炎热值
		foreach (var creature in base.CombatState.GetTeammatesOf(base.Owner.Creature))
		{
			if (creature.IsAlive && creature.IsPlayer)
			{
				await PowerCmd.Apply<HeatPower>(
					choiceContext, creature, heat, base.Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后炎热 16 -> 24 (+8)
		base.DynamicVars["HeatPower"].UpgradeValueBy(8m);
	}
}
