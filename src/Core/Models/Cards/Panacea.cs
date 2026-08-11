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
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 万灵药：失去至多 20 点炎热、20 层中毒、20 层孢子、1 层虚弱、1 层脆弱、1 层易伤，回复 8（12）点生命值。
/// 2 费，技能牌，稀有稀有度，目标自身，消耗，食物牌。
/// </summary>
public sealed class Panacea : CardModel, IFoodCard
{
	// 卡面图片（文件名与卡牌 ID 一致：panacea.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/panacea.png");
	

	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础回复 8 点生命值（升级后 12 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new HealVar(8m)
	};

	// 悬停提示：显示中毒的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ZhongduPower>()
	};

	public Panacea()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 失去至多 20 点炎热
		HeatPower? heat = base.Owner.Creature.GetPower<HeatPower>();
		if (heat != null && heat.Amount > 0)
		{
			int reduceHeat = (int)Math.Min(heat.Amount, 20m);
			await PowerCmd.ModifyAmount(choiceContext, heat, -reduceHeat, base.Owner.Creature, this);
		}

		// 2. 失去至多 20 层中毒
		ZhongduPower? zhongdu = base.Owner.Creature.GetPower<ZhongduPower>();
		if (zhongdu != null && zhongdu.Amount > 0)
		{
			int reduceZhongdu = (int)Math.Min(zhongdu.Amount, 20m);
			await PowerCmd.ModifyAmount(choiceContext, zhongdu, -reduceZhongdu, base.Owner.Creature, this);
		}

		// 3. 失去至多 20 层孢子
		SporePower? spore = base.Owner.Creature.GetPower<SporePower>();
		if (spore != null && spore.Amount > 0)
		{
			int reduceSpore = (int)Math.Min(spore.Amount, 20m);
			await PowerCmd.ModifyAmount(choiceContext, spore, -reduceSpore, base.Owner.Creature, this);
		}

		// 4. 失去至多 1 层虚弱
		WeakPower? weak = base.Owner.Creature.GetPower<WeakPower>();
		if (weak != null && weak.Amount > 0)
		{
			int reduceWeak = (int)Math.Min(weak.Amount, 1m);
			await PowerCmd.ModifyAmount(choiceContext, weak, -reduceWeak, base.Owner.Creature, this);
		}

		// 5. 失去至多 1 层脆弱
		FrailPower? frail = base.Owner.Creature.GetPower<FrailPower>();
		if (frail != null && frail.Amount > 0)
		{
			int reduceFrail = (int)Math.Min(frail.Amount, 1m);
			await PowerCmd.ModifyAmount(choiceContext, frail, -reduceFrail, base.Owner.Creature, this);
		}

		// 6. 失去至多 1 层易伤
		VulnerablePower? vulnerable = base.Owner.Creature.GetPower<VulnerablePower>();
		if (vulnerable != null && vulnerable.Amount > 0)
		{
			int reduceVulnerable = (int)Math.Min(vulnerable.Amount, 1m);
			await PowerCmd.ModifyAmount(choiceContext, vulnerable, -reduceVulnerable, base.Owner.Creature, this);
		}

		// 7. 回复 8（12）点生命值
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
	}

	protected override void OnUpgrade()
	{
		// 升级后回复 8 -> 12 (+4)
		base.DynamicVars.Heal.UpgradeValueBy(4m);
	}
}
