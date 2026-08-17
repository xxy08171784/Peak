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
/// 历练：获得能力"历练"：每当你失去一次负面状态时，获得 1 点力量（按次数，不按层数）。
/// 1 费，能力牌，稀有稀有度，目标自身。升级后获得保留。
/// </summary>
public sealed class ExperienceAndToughening : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/experience_and_toughening.png");

	// 悬停预览：显示历练的机制说明
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
	{
		HoverTipFactory.FromPower<ExperienceAndTougheningPower>()
	};

	// 基础变量：每次失去 debuff 获得 1 点力量
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new PowerVar<ExperienceAndTougheningPower>(1m)
	};

	public ExperienceAndToughening()
		: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 播放能力强化动画
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

		// 赋予玩家历练能力，每次失去 debuff 获得 1 点力量
		await PowerCmd.Apply<ExperienceAndTougheningPower>(
			choiceContext,
			base.Owner.Creature,
			base.DynamicVars["ExperienceAndTougheningPower"].BaseValue,
			base.Owner.Creature,
			this
		);
	}

	protected override void OnUpgrade()
	{
		// 升级后获得固有
		AddKeyword(CardKeyword.Innate);
	}
}
