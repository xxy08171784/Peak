using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 纵火高手：获得 1 层纵火高手。
/// 纵火高手：HeatPower 回合结束伤害翻倍。
/// 1 费，升级后 0 费，无固有词条。
/// </summary>
public sealed class ArsonExpert : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ArsonExpertPower>(),
		HoverTipFactory.FromPower<HeatPower>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ArsonExpertPower>(1m)
	};

	public ArsonExpert()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 给自己施加 1 层纵火高手 Buff
		await PowerCmd.Apply<ArsonExpertPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["ArsonExpertPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	// 升级：能耗 1 -> 0
	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
