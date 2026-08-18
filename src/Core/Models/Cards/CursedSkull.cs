using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 诅咒头骨：（保留）消耗，失去 6 最大生命值，失去 999 生命，使所有队友回复满生命。
/// 1 费，技能牌，稀有稀有度，目标自身。
/// 多人专属卡牌。
/// </summary>
public sealed class CursedSkull : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	// 保留 + 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
	{
		CardKeyword.Retain,
		CardKeyword.Exhaust
	};

	// 卡面尚未绘制，暂用 beta 占位图
	public override string PortraitPath => CardModel.MissingPortraitPath;

	public CursedSkull()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 失去 6 点最大生命值
		await CreatureCmd.LoseMaxHp(choiceContext, base.Owner.Creature, 6m, true);

		// 2. 失去 999 生命（直接失去，不触发无实体等，可被格挡也无意义）
		await CreatureCmd.Damage(
			choiceContext,
			base.Owner.Creature,
			999m,
			ValueProp.Unpowered | ValueProp.Move,
			base.Owner.Creature);

		// 3. 所有队友回复满生命（自己除外）
		foreach (var teammate in base.CombatState.GetTeammatesOf(base.Owner.Creature))
		{
			if (teammate.IsAlive && teammate.IsPlayer && teammate != base.Owner.Creature)
			{
				// Heal 内部会钳制到最大生命值，回复满血
				await CreatureCmd.Heal(teammate, teammate.MaxHp);
			}
		}
	}
}
