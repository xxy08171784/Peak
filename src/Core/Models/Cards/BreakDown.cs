using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 瓦解：失去所有覆甲，每失去一层覆甲就对所有敌人造成 3（5）点伤害一次。
/// 覆甲会一层一层消失，每消失一层造成一次伤害（连击效果）。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class BreakDown : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/break_down.png");

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<PlatingPower>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(3m, ValueProp.Move)
	};

	public BreakDown()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 获取当前覆甲层数（不立即移除，由连击过程中逐层扣除）
		PlatingPower? plating = base.Owner.Creature.GetPower<PlatingPower>();
		int layersLost = plating != null && plating.Amount > 0 ? plating.Amount : 0;

		if (layersLost > 0)
		{
			// 覆甲一层一层消失：每次造成伤害前扣除一层覆甲，形成连击
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
				.WithHitCount(layersLost)
				.FromCard(this, cardPlay)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_blunt")
				.WithWaitBeforeHit(0.08f, 0.12f)
				.BeforeDamage(async () =>
				{
					// 每次造成伤害前，扣减一层覆甲（覆甲逐层消失）
					if (plating != null && plating.Amount > 0)
					{
						await PowerCmd.ModifyAmount(choiceContext, plating, -1m, null, null);
					}
				})
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 3 -> 5 (+2)
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
