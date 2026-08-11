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
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 烤炉：获得 15 点炎热，烤所有手牌。
/// 烤：如果是食物卡，将其升级；如果是其他卡，将其消耗。
/// 2 费（升级后 1 费），技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class Oven : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：oven.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/oven.png");

	// 动态变量：基础获得 15 点炎热
	protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new PowerVar<HeatPower>(15m)
	};

	// 悬停提示：显示炎热的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<HeatPower>()
	};

	public Oven()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 获得 15 点炎热
		await PowerCmd.Apply<HeatPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["HeatPower"].BaseValue,
			base.Owner.Creature,
			this
		);

		// 2. 烤所有手牌：食物卡升级，其他卡消耗
		// 先快照列表，避免烤牌过程中（消耗）修改手牌集合导致"集合已修改"异常
		foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards.ToList())
		{
			// 记录"被烤"状态（供大炮等卡牌打出时判断）
			RoastTracker.MarkRoasted(card);

			if (card is Cannon)
			{
				// 大炮被烤 -> 自动打出：由大炮 OnPlay 判断被烤状态
				await CardCmd.AutoPlay(choiceContext, card, null);
			}
			else if (card is IFoodCard)
			{
				// 其他食物卡 -> 升级
				CardCmd.Upgrade(card);
			}
			else
			{
				// 其他卡 -> 消耗
				await CardCmd.Exhaust(choiceContext, card);
			}
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 2 -> 1 (-1)
		base.EnergyCost.UpgradeBy(-1);
	}
}
