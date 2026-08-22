using System.Collections.Generic;
using System.Linq;
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
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

/// <summary>
/// 方山：切换到方山场景（获得 15 层炎热），额外获得 10（15）点炎热。
/// 1 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class TheMountainOfFlames : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/the_mountain_of_flames.png");

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<HeatPower>()
	};

	// 基础变量：额外获得 10 点炎热（升级后 15 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<HeatPower>(10m)
	};

	public TheMountainOfFlames()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 切换到方山场景（场景 2），触发场景效果（获得 15 层炎热）
		MyClimbing? myClimbing = base.Owner.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing != null)
		{
			int currentScene = myClimbing.DisplayAmount;
			int targetScene = 2; // 方山
			int delta = (targetScene - currentScene + 4) % 4;
			if (delta != 0)
			{
				await myClimbing.ModifyEnvironmentValue(choiceContext, delta);
			}
			else
			{
				// 已在方山场景，直接触发场景效果
				await myClimbing.ExecuteStateEffect(choiceContext, targetScene);
			}
		}

		// 2. 额外获得 10（15）点炎热
		await PowerCmd.Apply<HeatPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["HeatPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后额外炎热 10 -> 15 (+5)
		base.DynamicVars["HeatPower"].UpgradeValueBy(5m);
	}
}
