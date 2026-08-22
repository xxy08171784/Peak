using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 毒喇叭菇：获得 2（3）点能量，给予自己 6 层中毒。
/// 0 费，食物牌（技能类型 + 食物接口），罕见稀有度，目标自身。
/// </summary>
public sealed class PoisonTrumpetMushroom : CardModel, IFoodCard
{
	// 动态变量：基础能量 2（升级后 3）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(2),
		new PowerVar<ZhongduPower>(6m)
	};

	// 悬停提示：显示中毒的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		base.EnergyHoverTip,
		HoverTipFactory.FromPower<ZhongduPower>()
	};

	public PoisonTrumpetMushroom()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 2 点能量
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);

		// 2. 给予自己 6 层中毒
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
		// 升级后能量 2 -> 3 (+1)
		base.DynamicVars["Energy"].UpgradeValueBy(1m);
	}
}
