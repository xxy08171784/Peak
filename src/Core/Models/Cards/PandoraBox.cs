using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 潘多拉餐盒：失去所有费用和 debuff，获得随机的费用，状态，buff条。
/// 2 费，食物牌（技能类型 + 食物接口），稀有稀有度，目标自身，消耗（升级后新增保留）。
///
/// 随机费用：0-4
/// 随机状态（力量：-1~3，敏捷：-1~3，再生：0~4，虚弱：0~1，脆弱：0~1，易伤：0~1，缓冲：0~1）
/// 随机buff（孢子：0-20，中毒：0-10，炎热：0-20）
/// </summary>
public sealed class PandoraBox : CardModel, IFoodCard
{
	// 卡面图片（文件名与卡牌 ID 不一致，需显式指定）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/Pandora_s_box.png");

	protected override string PortraitPngPath => ImageHelper.GetImagePath("packed/card_portraits/scout/Pandora_s_box.png");

	// 固有词条：消耗（升级后通过 OnUpgrade 添加保留）
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	public PandoraBox()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 使用 System.Random 生成随机值
		Random rng = new Random();

		// 1. 失去所有费用（当前能量置零）
		await PlayerCmd.LoseEnergy(base.Owner.PlayerCombatState.Energy, base.Owner);

		// 2. 移除所有 debuff（负面状态），保留正面 buff
		List<PowerModel> debuffsToRemove = base.Owner.Creature.Powers
			.Where((PowerModel p) => p.Type == PowerType.Debuff)
			.ToList();
		foreach (PowerModel debuff in debuffsToRemove)
		{
			await PowerCmd.Remove(debuff);
		}

		// 3. 获得随机费用（0-4）
		decimal randomEnergy = rng.Next(0, 5);
		await PlayerCmd.GainEnergy(randomEnergy, base.Owner);

		// 4. 获得随机状态
		// 力量：-1~3
		int strength = rng.Next(-1, 4);
		if (strength != 0)
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, Math.Abs(strength), base.Owner.Creature, this);
			if (strength < 0)
			{
				// 如果是负数，找到刚加的 StrengthPower 设为负数
				StrengthPower? sp = base.Owner.Creature.GetPower<StrengthPower>();
				if (sp != null)
				{
					await PowerCmd.ModifyAmount(choiceContext, sp, -strength * 2, base.Owner.Creature, this);
				}
			}
		}

		// 敏捷：-1~3
		int dexterity = rng.Next(-1, 4);
		if (dexterity != 0)
		{
			await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, Math.Abs(dexterity), base.Owner.Creature, this);
			if (dexterity < 0)
			{
				DexterityPower? dp = base.Owner.Creature.GetPower<DexterityPower>();
				if (dp != null)
				{
					await PowerCmd.ModifyAmount(choiceContext, dp, -dexterity * 2, base.Owner.Creature, this);
				}
			}
		}

		// 再生：0~4
		int regen = rng.Next(0, 5);
		if (regen > 0)
		{
			await PowerCmd.Apply<RegenPower>(choiceContext, base.Owner.Creature, regen, base.Owner.Creature, this);
		}

		// 虚弱：0~1
		int weak = rng.Next(0, 2);
		if (weak > 0)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, base.Owner.Creature, weak, base.Owner.Creature, this);
		}

		// 脆弱：0~1
		int frail = rng.Next(0, 2);
		if (frail > 0)
		{
			await PowerCmd.Apply<FrailPower>(choiceContext, base.Owner.Creature, frail, base.Owner.Creature, this);
		}

		// 易伤：0~1
		int vulnerable = rng.Next(0, 2);
		if (vulnerable > 0)
		{
			await PowerCmd.Apply<VulnerablePower>(choiceContext, base.Owner.Creature, vulnerable, base.Owner.Creature, this);
		}

		// 缓冲：0~1
		int buffer = rng.Next(0, 2);
		if (buffer > 0)
		{
			await PowerCmd.Apply<BufferPower>(choiceContext, base.Owner.Creature, buffer, base.Owner.Creature, this);
		}

		// 5. 获得随机 buff
		// 孢子：0~20
		int spore = rng.Next(0, 21);
		if (spore > 0)
		{
			await PowerCmd.Apply<SporePower>(choiceContext, base.Owner.Creature, spore, base.Owner.Creature, this);
		}

		// 中毒：0~10
		int zhongdu = rng.Next(0, 11);
		if (zhongdu > 0)
		{
			await PowerCmd.Apply<ZhongduPower>(choiceContext, base.Owner.Creature, zhongdu, base.Owner.Creature, this);
		}

		// 炎热：0~20
		int heat = rng.Next(0, 21);
		if (heat > 0)
		{
			await PowerCmd.Apply<HeatPower>(choiceContext, base.Owner.Creature, heat, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 → 1（-1），并新增保留词条
		base.EnergyCost.UpgradeBy(-1);
		AddKeyword(CardKeyword.Retain);
	}
}