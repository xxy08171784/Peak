using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

/// <summary>
/// 煎蛋：获得 7 点格挡。
/// 1 费，食物牌（技能类型 + 食物接口），普通稀有度，目标自身。
/// </summary>
public sealed class RoastEgg : CardModel, IFoodCard
{
	// 获得格挡，便于机制识别
	public override bool GainsBlock => true;

	// 动态变量：基础格挡 7 点
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new BlockVar(7m, ValueProp.Move) };

	public RoastEgg()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后格挡不变（表格里煎蛋只有基础值）
	}
}
