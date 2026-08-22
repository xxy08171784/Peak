using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 灵药菇：获得 3（4）点再生。
/// 1 费，技能牌，罕见稀有度，目标自身，消耗。
/// </summary>
public sealed class ElixirMushroom : CardModel, IItemCard
{
	// 卡面图片（文件名与卡牌 ID 一致：elixir_mushroom.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/elixir_mushroom.png");

	

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础再生 3 点（升级后 4 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<RegenPower>(3m)
	};

	// 悬停提示：显示再生的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<RegenPower>()
	};

	public ElixirMushroom()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 获得 3（4）点再生
		await PowerCmd.Apply<RegenPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["RegenPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后再生 3 -> 4 (+1)
		base.DynamicVars["RegenPower"].UpgradeValueBy(1m);
	}
}
