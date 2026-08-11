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
/// 自助餐：能力牌，获得自助餐 Power。
/// 每回合第一次抽到食物卡时，额外抽 2 张牌。
/// 1 费（升级后 0 费），能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Buffet : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：buffet.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/buffet.png");

	// 动态变量：额外抽 2 张牌
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<BuffetPower>(2m)
	};

	// 悬停提示：显示自助餐的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<BuffetPower>()
	};

	public Buffet()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<BuffetPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["BuffetPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
