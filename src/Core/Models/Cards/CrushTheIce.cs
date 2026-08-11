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
/// 碎冰：获得能力"碎冰"：对拥有渐冻的敌人造成的伤害提高 50%（75%）。
/// 1 费，能力牌，稀有稀有度，目标自身。
/// </summary>
public sealed class CrushTheIce : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/crush_the_ice.png");

	// 悬停预览：显示渐冻和碎冰的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<FrostbitePower>(),
		HoverTipFactory.FromPower<CrushTheIcePower>()
	};

	// 基础变量：增伤 50%（升级后 75%）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<CrushTheIcePower>(50m)
	};

	public CrushTheIce()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 播放能力强化动画
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

		// 赋予玩家碎冰能力，增伤百分比 = 50（75）
		await PowerCmd.Apply<CrushTheIcePower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["CrushTheIcePower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后增伤 50% -> 75% (+25)
		base.DynamicVars["CrushTheIcePower"].UpgradeValueBy(25m);
	}
}
