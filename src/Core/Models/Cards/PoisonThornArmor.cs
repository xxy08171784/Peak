using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 毒刺护体：能力牌，获得毒刺 Power。获得 2 层中毒。
/// 当被敌人攻击命中时，反击该名敌人 3（4）点中毒。
/// 2 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class PoisonThornArmor : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：poison_thorn_armor.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/poison_thorn_armor.png");

	

	// 动态变量：自己获得 2 层中毒；毒刺反击 3 点中毒（升级后 4 点）。
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ZhongduPower>(2m),
		new PowerVar<TelsonPower>(3m)
	};

	// 悬停提示：按卡面顺序显示中毒与毒刺说明。
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ZhongduPower>(),
		HoverTipFactory.FromPower<TelsonPower>()
	};

	public PoisonThornArmor()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 2 层中毒
		await PowerCmd.Apply<ZhongduPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["ZhongduPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 获得毒刺 Power
		await PowerCmd.Apply<TelsonPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["TelsonPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后毒刺反击 3 -> 4 (+1)
		base.DynamicVars["TelsonPower"].UpgradeValueBy(1m);
	}
}
