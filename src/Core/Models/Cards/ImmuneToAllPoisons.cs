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
/// 百毒不侵：能力牌，获得百毒不侵 Power。
/// 中毒不会对自己造成伤害，并且每当你失去 1 层中毒，就给予所有敌人 2 层中毒。
/// 1 费，能力牌，罕见稀有度，目标自身。升级后获得固有。
/// </summary>
public sealed class ImmuneToAllPoisons : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：immune_to_all_poisons.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/immune_to_all_poisons.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/immune_to_all_poisons.png");

	// 动态变量：基础给予敌人的中毒层数为 2 倍
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ImmuneToAllPoisonsPower>(2m)
	};

	// 悬停提示：显示百毒不侵的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ImmuneToAllPoisonsPower>()
	};

	public ImmuneToAllPoisons()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<ImmuneToAllPoisonsPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["ImmuneToAllPoisonsPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后获得固有词条
		AddKeyword(CardKeyword.Innate);
	}
}
