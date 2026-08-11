using System.Collections.Generic;
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

namespace peak.Core.Models.Cards;

/// <summary>
/// 雪甲：获得能力"雪甲"：每当给予一层寒冷时，获得 2（3）点格挡。
/// 1 费，能力牌，普通稀有度，目标自身。
/// </summary>
public sealed class SnowArmor : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/snow_armor.png");

	// 悬停预览：显示寒冷和雪甲的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ColdPower>(),
		HoverTipFactory.FromPower<SnowArmorPower>()
	};

	// 基础变量：每层寒冷获得 2 点格挡（升级后 3 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<SnowArmorPower>(2m)
	};

	public SnowArmor()
		: base(1, CardType.Power, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 播放能力强化动画
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

		// 赋予玩家雪甲能力，每层寒冷获得 2（3）点格挡
		await PowerCmd.Apply<SnowArmorPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["SnowArmorPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每层寒冷格挡 2 -> 3 (+1)
		base.DynamicVars["SnowArmorPower"].UpgradeValueBy(1m);
	}
}
