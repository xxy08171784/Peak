using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
/// 饱腹拳：造成 8（11）点伤害，如果上一张打出的牌是食物牌，则额外给予 1（2）层易伤。
/// 1 费，攻击牌，普通稀有度，目标任意敌人。
/// </summary>
public sealed class FullBellyPunch : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：full_belly_punch.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/full_belly_punch.png");

	// 动态变量：基础伤害 8 点（升级后 11 点）、易伤 1 层（升级后 2 层）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(8m, ValueProp.Move),
		new PowerVar<VulnerablePower>(1m)
	};

	// 悬停提示：显示易伤的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<VulnerablePower>()
	};

	public FullBellyPunch()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 造成 8（11）点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		// 2. 检查上一张打出的牌是否为食物牌
		// 注意：自己尚未进入 CardPlaysFinished，所以 LastOrDefault 取到的是"上一张已完成的牌"
		CardPlayFinishedEntry? last = CombatManager.Instance.History.CardPlaysFinished
			.LastOrDefault(e => e.CardPlay.Card.Owner == base.Owner);

		if (last != null && last.CardPlay.Card is IFoodCard)
		{
			// 3. 额外给予 1（2）层易伤
			await PowerCmd.Apply<VulnerablePower>(
				choiceContext,
				cardPlay.Target,
				base.DynamicVars["VulnerablePower"].BaseValue,
				base.Owner.Creature,
				this
			);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 8 -> 11 (+3)
		base.DynamicVars.Damage.UpgradeValueBy(3m);
		// 升级后易伤 1 -> 2 (+1)
		base.DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
	}
}
