using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 绿脆莓：消耗，回复 5 点生命，给予自己 3（1）层中毒（自定义 ZhongduPower，回合结束触发）。
/// 1 费，食物牌（技能类型 + 食物接口），普通稀有度，目标自身。
/// </summary>
public sealed class GreenBerry : CardModel, IFoodCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ZhongduPower>()
	};

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础回复 5 点、基础中毒 3 层（升级后降为 1 层）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new HealVar(5m),
		new PowerVar<ZhongduPower>(3m)
	};

	public GreenBerry()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 回复 5 点生命
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Heal"].BaseValue);

		// 2. 给自己 3 层自定义中毒（ZhongduPower，回合结束时触发）
		await PowerCmd.Apply<ZhongduPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["ZhongduPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后中毒从 3 层降为 1 层
		base.DynamicVars["ZhongduPower"].UpgradeValueBy(-2m);
	}
}
