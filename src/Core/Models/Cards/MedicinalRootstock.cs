using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 药用根茎：减少 5（8）层中毒，获得 7（9）点格挡。
/// 1 费，食物牌（技能类型 + 食物接口），普通稀有度，目标自身。
/// </summary>
public sealed class MedicinalRootstock : CardModel, IFoodCard
{
	// 动态变量：基础减少 5 层中毒（升级后 8 层）、基础获得 7 点格挡（升级后 9 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ZhongduPower>(5m),
		new BlockVar(7m, ValueProp.Move)
	};

	public MedicinalRootstock()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 减少 ZhongduPower（中毒）层数
		ZhongduPower? zhongdu = base.Owner.Creature.GetPower<ZhongduPower>();
		if (zhongdu != null && zhongdu.Amount > 0)
		{
			int reduceAmount = (int)System.Math.Min(zhongdu.Amount, base.DynamicVars["ZhongduPower"].BaseValue);
			await PowerCmd.ModifyAmount(choiceContext, zhongdu, -reduceAmount, base.Owner.Creature, this);
		}

		// 2. 获得格挡
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后减少中毒 5 → 8（+3），获得格挡 7 → 9（+2）
		base.DynamicVars["ZhongduPower"].UpgradeValueBy(3m);
		base.DynamicVars.Block.UpgradeValueBy(2m);
	}
}