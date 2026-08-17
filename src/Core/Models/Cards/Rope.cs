using System;
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
/// 绳索：给予 1（2）层缠绕效果。
/// 1 费，技能牌，普通稀有度，目标任意敌人。
/// </summary>
public sealed class Rope : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：rope.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/rope.png");

	// 动态变量：缠绕 1 层（升级后 2 层）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<TwinePower>(1m)
	};

	// 悬停提示：显示缠绕的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<TwinePower>()
	};

	public Rope()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 给予 1（2）层缠绕
		await PowerCmd.Apply<TwinePower>(
			choiceContext,
			cardPlay.Target,
			base.DynamicVars["TwinePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后缠绕 1 -> 2 (+1)
		base.DynamicVars["TwinePower"].UpgradeValueBy(1m);
	}
}
