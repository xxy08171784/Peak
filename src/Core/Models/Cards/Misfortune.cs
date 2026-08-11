using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 厄运：获得能力"厄运"：每当你给予敌人一次负面状态时，对随机敌人造成 7（9）点伤害。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Misfortune : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/misfortune.png");

	// 悬停预览：显示厄运的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<MisfortunePower>()
	};

	// 基础变量：每次触发 7 点伤害（升级后 9 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(7m, ValueProp.Unpowered)
	};

	public Misfortune()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 赋予玩家厄运能力，每次触发伤害 = 7（9）
		await PowerCmd.Apply<MisfortunePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars.Damage.BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 7 -> 9 (+2)
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
