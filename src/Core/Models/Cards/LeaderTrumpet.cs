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
/// 领队喇叭：获得 2（3）点能量，抽 2（3）张牌，获得 1 层领队追杀。
/// 领队追杀：在你的回合结束时，受到 5 点伤害。
/// 0 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class LeaderTrumpet : CardModel
{
	// 动态变量：基础能量 2、基础抽牌 2
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(2),
		new CardsVar(2)
	};

	// 悬停提示：显示领队追杀的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<LeadersPursuitPower>()
	};

	public LeaderTrumpet()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 2 点能量
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);

		// 2. 抽 2 张牌
		await CardPileCmd.Draw(
			choiceContext,
			base.DynamicVars["Cards"].BaseValue,
			base.Owner
		);

		// 3. 获得 5 层领队追杀（每层 = 1 点伤害，显示和实际一致）
		await PowerCmd.Apply<LeadersPursuitPower>(
			choiceContext,
			base.Owner.Creature,
			5m,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后能量 2 -> 3 (+1)，抽牌 2 -> 3 (+1)
		base.DynamicVars["Energy"].UpgradeValueBy(1m);
		base.DynamicVars["Cards"].UpgradeValueBy(1m);
	}
}
