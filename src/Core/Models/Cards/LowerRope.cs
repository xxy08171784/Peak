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
/// 放绳索：消耗，所有玩家获得 2 点敏捷。
/// 1 费（升级后 0 费），技能牌，罕见稀有度，目标全体友方。
/// 多人专属卡牌。
/// </summary>
public sealed class LowerRope : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<DexterityPower>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<DexterityPower>(2m)
	};

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 卡面图片（文件名：lower_rope.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/lower_rope.png");

	public LowerRope()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 所有玩家（自己 + 队友）获得 2 点敏捷
		foreach (var creature in base.CombatState.GetTeammatesOf(base.Owner.Creature))
		{
			if (creature.IsAlive && creature.IsPlayer)
			{
				await PowerCmd.Apply<DexterityPower>(
					choiceContext, creature, base.DynamicVars["DexterityPower"].BaseValue, base.Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
