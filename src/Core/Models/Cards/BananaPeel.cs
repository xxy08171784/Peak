using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

public sealed class BananaPeel : CardModel
{
	// 悬停提示：显示虚弱（WeakPower）说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => 
		new IHoverTip[] { HoverTipFactory.FromPower<WeakPower>() };

	// 基础变量：5点伤害，2层虚弱（固定2层）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(5m, ValueProp.Move),
		new PowerVar<WeakPower>(2m)
	};

	public BananaPeel()
		: base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		// 1. 对选中的目标敌人造成 5 点伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		// 2. 施加虚弱逻辑：
		if (!base.IsUpgraded)
		{
			// 【未升级】：仅对选中的目标敌人施加 2 层虚弱
			await PowerCmd.Apply<WeakPower>(
				choiceContext, 
				cardPlay.Target, 
				base.DynamicVars.Weak.BaseValue, 
				base.Owner.Creature, 
				this
			);
		}
		else
		{
			// 【升级后】：对全体敌人施加 2 层虚弱（不包括玩家自己）
			IEnumerable<Creature> enemies = base.CombatState.HittableEnemies.Where(c => c.IsAlive);

			foreach (Creature enemy in enemies)
			{
				await PowerCmd.Apply<WeakPower>(
					choiceContext, 
					enemy, 
					base.DynamicVars.Weak.BaseValue, 
					base.Owner.Creature, 
					this
				);
			}
		}
	}

	// 升级逻辑：提升的是作用范围（单体敌人 -> 全体敌人），虚弱固定为 2 层
	protected override void OnUpgrade()
	{
	}

	/// <summary>
	/// 静态生成函数：向玩家手牌加入香蕉皮
	/// </summary>
	public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, bool isUpgraded, ICombatState combatState)
	{
		if (count == 0 || CombatManager.Instance.IsOverOrEnding)
		{
			return Array.Empty<CardModel>();
		}

		List<CardModel> peels = new List<CardModel>();
		for (int i = 0; i < count; i++)
		{
			BananaPeel peel = combatState.CreateCard<BananaPeel>(owner);
			if (isUpgraded)
			{
				peel.UpgradeInternal();
			}
			peels.Add(peel);
		}

		await CardPileCmd.AddGeneratedCardsToCombat(peels, PileType.Hand, owner);
		return peels;
	}
}