using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;
using System.Linq;

namespace peak.Core.Models.Cards;

/// <summary>
/// 闯关高手：将场景增加 X（升级后 X+1）次。
/// X 费，技能牌，稀有稀有度，目标自身。
/// </summary>
public sealed class StageClearMaster : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/stage_clear_master.png");

	// X 费卡
	protected override bool HasEnergyCostX => true;

	public StageClearMaster()
		: base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		MyClimbing? myClimbing = base.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing == null)
		{
			return;
		}

		// 没升级 = X 次，升级后 = X+1 次
		int xValue = ResolveEnergyXValue();
		int times = xValue + (base.IsUpgraded ? 1 : 0);

		for (int i = 0; i < times; i++)
		{
			await myClimbing.ModifyEnvironmentValue(choiceContext, 1);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级：无特殊变化（X 费）
	}
}
