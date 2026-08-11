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
/// 森蕈：切换到森蕈场景（将一张带虚无的蘑菇盲盒加入手牌），获得 5 层孢子。
/// 0 费，技能牌，普通稀有度，目标自身。
/// </summary>
public sealed class ForestMushroom : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/forest_mushroom.png");

	// 悬停预览：显示孢子的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SporePower>()
	};

	// 基础变量：获得 5 层孢子
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SporePower>(5m)
	};

	public ForestMushroom()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 切换到森蕈场景（场景 1），触发场景效果（获得一张蘑菇盲盒）
		MyClimbing? myClimbing = base.Owner.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing != null)
		{
			int currentScene = myClimbing.DisplayAmount;
			int targetScene = 1; // 森蕈
			int delta = (targetScene - currentScene + 4) % 4;
			if (delta != 0)
			{
				await myClimbing.ModifyEnvironmentValue(choiceContext, delta);
			}
			else
			{
				// 已在森蕈场景，直接触发场景效果
				await myClimbing.ExecuteStateEffect(choiceContext, targetScene);
			}
		}

		// 2. 获得 5 层孢子
		await PowerCmd.Apply<SporePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SporePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 暂无升级效果
	}
}
