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
/// 冰露：消耗，失去 25（35）点炎热值。
/// 失去炎热值会触发 HeatPower 对随机一名敌人造成等量伤害。
/// 1 费，食物牌（技能类型 + 食物接口），罕见稀有度，目标自身。
/// </summary>
public sealed class IceDew : CardModel, IFoodCard
{
	// 消耗关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	// 动态变量：基础失去 25 点炎热值（升级后 35 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<HeatPower>(25m)
	};

	// 悬停提示：显示炎热值（HeatPower）的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<HeatPower>()
	};

	public IceDew()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 减少自身炎热值（传入负数）。减少会触发 HeatPower 对随机敌人造成等量伤害。
		HeatPower? heatPower = base.Owner.Creature.GetPower<HeatPower>();
		if (heatPower != null && heatPower.Amount > 0)
		{
			int reduceAmount = (int)System.Math.Min(heatPower.Amount, base.DynamicVars["HeatPower"].BaseValue);
			await PowerCmd.ModifyAmount(choiceContext, heatPower, -reduceAmount, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后失去炎热值从 25 提升到 35 (+10)
		base.DynamicVars["HeatPower"].UpgradeValueBy(10m);
	}
}
