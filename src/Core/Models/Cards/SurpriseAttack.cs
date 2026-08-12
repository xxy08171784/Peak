using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Relics;

namespace peak.Core.Models.Cards;

/// <summary>
/// 天降奇兵：从四个场景中选择一个切换过去。
/// 1 费（升级后 0 费），技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class SurpriseAttack : CardModel
{
	// 卡面图片
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/surprise_attack.png");

	public SurpriseAttack()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 使用已有的场景卡作为选择选项（仅用于选择界面展示，不会真正打出它们）
		List<CardModel> choices = new()
		{
			base.CombatState.CreateCard<Climb>(base.Owner),
			base.CombatState.CreateCard<ForestMushroom>(base.Owner),
			base.CombatState.CreateCard<TheMountainOfFlames>(base.Owner),
			base.CombatState.CreateCard<Snowstorm>(base.Owner),
		};

		// 2. 让玩家选择一张场景卡
		CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
			context: choiceContext,
			cardsIn: choices,
			player: base.Owner,
			prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1)
		)).FirstOrDefault();

		if (selected == null)
		{
			return;
		}

		// 3. 根据选中的卡切换到对应的场景
		MyClimbing? myClimbing = base.Owner?.Relics.OfType<MyClimbing>().FirstOrDefault();
		if (myClimbing == null)
		{
			return;
		}

		int targetScene = selected switch
		{
			Climb => 0,               // 海岛
			ForestMushroom => 1,      // 森蕈
			TheMountainOfFlames => 2, // 方山
			Snowstorm => 3,           // 雪山
			_ => -1
		};

		if (targetScene >= 0)
		{
			await myClimbing.SetEnvironmentValue(choiceContext, targetScene);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}
