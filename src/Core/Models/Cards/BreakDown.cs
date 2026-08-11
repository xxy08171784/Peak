using System;
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
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 瓦解：失去所有覆甲，每失去一层覆甲就对所有敌人造成 4（6）点伤害一次。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class BreakDown : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/break_down.png");

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(4m, ValueProp.Move)
	};

	public BreakDown()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 失去所有覆甲，计算层数
		PlatingPower? plating = base.Owner.Creature.GetPower<PlatingPower>();
		int layersLost = 0;
		if (plating != null && plating.Amount > 0)
		{
			layersLost = plating.Amount;
			await PowerCmd.Remove(plating);
		}

		if (layersLost > 0)
		{
			// 每失去一层覆甲，对所有敌人造成 4（6）点伤害（总伤害 = 层数 * 4/6）
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue * layersLost)
				.FromCard(this)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 4 -> 6 (+2)
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}