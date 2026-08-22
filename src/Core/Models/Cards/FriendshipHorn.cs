using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Cards;

/// <summary>
/// 友谊喇叭：所有玩家获得 3（4）点费用，每打出一次该牌费用 +1。
/// 0 费，技能牌，稀有稀有度，目标全体友方。
/// 多人专属卡牌。
/// </summary>
public sealed class FriendshipHorn : CardModel, IItemCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		base.EnergyHoverTip
	};

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 卡面尚未绘制，暂用 beta 占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	// 动态变量：获得能量 3（升级后 4）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(3)
	};

	public FriendshipHorn()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal energy = base.DynamicVars["Energy"].BaseValue;

		// 1. 所有玩家（自己 + 队友）获得能量
		foreach (var creature in base.CombatState.GetTeammatesOf(base.Owner.Creature))
		{
			if (creature.IsAlive && creature.IsPlayer)
			{
				await PlayerCmd.GainEnergy(energy, creature.Player!);
			}
		}

		// 2. 每打出一次该牌，本场战斗费用 +1
		base.EnergyCost.AddThisCombat(1);
	}

	protected override void OnUpgrade()
	{
		// 升级后能量 3 -> 4 (+1)
		base.DynamicVars["Energy"].UpgradeValueBy(1m);
	}
}
