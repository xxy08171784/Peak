using System.Collections.Generic;
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
/// 秘制辣酱：能力牌，获得秘制辣酱 Power。
/// 每当你打出一张食物牌，获得 6 点炎热值。
/// 1 费（升级后 0 费），能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class SecretRecipeChiliSauce : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：secret_recipe_chili_sauce.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/secret_recipe_chili_sauce.png");
	

	// 动态变量：基础每次获得 9 点炎热值
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SecretRecipeChiliSaucePower>(9m)
	};

	// 悬停提示：显示秘制辣酱的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<SecretRecipeChiliSaucePower>(),
		HoverTipFactory.FromPower<HeatPower>()
	};

	public SecretRecipeChiliSauce()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<SecretRecipeChiliSaucePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SecretRecipeChiliSaucePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后费用 1 -> 0
		base.EnergyCost.UpgradeBy(-1);
	}
}
