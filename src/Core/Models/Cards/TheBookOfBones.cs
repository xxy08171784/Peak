using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 骸骨之书：保留。失去 5 点生命上限，移除所有负面状态，给予自己 99 层易伤，回复满生命。
/// 0 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class TheBookOfBones : CardModel
{
	// 保留关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	public TheBookOfBones()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 失去 5 点生命上限
		await CreatureCmd.LoseMaxHp(choiceContext, base.Owner.Creature, 5m, isFromCard: true);

		// 2. 移除所有负面状态（Debuff）
		//    注意：需要先收集再移除，避免遍历时修改集合
		List<PowerModel> debuffsToRemove = base.Owner.Creature.Powers
			.Where((PowerModel p) => p.Type == PowerType.Debuff)
			.ToList();
		foreach (PowerModel debuff in debuffsToRemove)
		{
			await PowerCmd.Remove(debuff);
		}

		// 3. 给予自己 99 层易伤
		await PowerCmd.Apply<VulnerablePower>(
			choiceContext,
			base.Owner.Creature,
			99m,
			base.Owner.Creature,
			this
		);

		// 4. 回复满生命
		decimal missingHp = base.Owner.Creature.MaxHp - base.Owner.Creature.CurrentHp;
		if (missingHp > 0m)
		{
			await CreatureCmd.Heal(base.Owner.Creature, missingHp);
		}
	}
}
