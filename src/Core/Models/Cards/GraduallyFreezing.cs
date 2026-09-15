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
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 渐冻：获得能力"渐冻"：在你的回合结束时，对所有拥有渐冻的敌人造成 12（16）点伤害。
/// 1 费，能力牌，罕见稀有度，目标自身。
/// </summary>
public sealed class GraduallyFreezing : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/gradually_freezing.png");

	// 悬停预览：显示渐冻和渐动的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<FrostbitePower>(),
		HoverTipFactory.FromPower<GraduallyFreezingPower>()
	};

	// 基础变量：每回合 12 点伤害（升级后 16 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DamageVar(12m, ValueProp.Unpowered)
	};

	public GraduallyFreezing()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 播放能力强化动画
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

		// 赋予玩家渐冻能力，每回合伤害值 = 12（16）
		await PowerCmd.Apply<GraduallyFreezingPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars.Damage.BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后每回合伤害 12 -> 16 (+4)
		base.DynamicVars.Damage.UpgradeValueBy(4m);
	}
}
