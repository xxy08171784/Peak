using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 有难同当：将自己的负面状态同步给指定敌人。
/// 孢子、炎热转化为灾厄；负值力量、敏捷同步降低。
/// 1 费，技能牌，稀有稀有度，目标任意敌人，保留。升级后对所有敌人生效。
/// </summary>
public sealed class ShareMisfortune : CardModel
{
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/share_misfortune.png");

	// 保留关键词
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

	// 升级后目标变为所有敌人
	public override TargetType TargetType =>
		base.IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

	public ShareMisfortune()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature player = base.Owner.Creature;

		// 确定目标：升级后全体敌人，否则指定敌人
		IEnumerable<Creature> targets = base.IsUpgraded
			? base.CombatState.HittableEnemies
			: new[] { cardPlay.Target! };

		foreach (Creature target in targets)
		{
			// 易伤
			VulnerablePower? vuln = player.GetPower<VulnerablePower>();
			if (vuln?.Amount > 0)
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target, vuln.Amount, player, this);

			// 脆弱
			FrailPower? frail = player.GetPower<FrailPower>();
			if (frail?.Amount > 0)
				await PowerCmd.Apply<FrailPower>(choiceContext, target, frail.Amount, player, this);

			// 虚弱
			WeakPower? weak = player.GetPower<WeakPower>();
			if (weak?.Amount > 0)
				await PowerCmd.Apply<WeakPower>(choiceContext, target, weak.Amount, player, this);

			// 负值力量：降低敌人等量力量
			StrengthPower? str = player.GetPower<StrengthPower>();
			if (str?.Amount < 0)
			{
				StrengthPower? targetStr = target.GetPower<StrengthPower>()
					?? await PowerCmd.Apply<StrengthPower>(choiceContext, target, 0, player, this);
				await PowerCmd.ModifyAmount(choiceContext, targetStr, str.Amount, player, this);
			}

			// 负值敏捷：降低敌人等量敏捷
			DexterityPower? dex = player.GetPower<DexterityPower>();
			if (dex?.Amount < 0)
			{
				DexterityPower? targetDex = target.GetPower<DexterityPower>()
					?? await PowerCmd.Apply<DexterityPower>(choiceContext, target, 0, player, this);
				await PowerCmd.ModifyAmount(choiceContext, targetDex, dex.Amount, player, this);
			}

			// 中毒
			ZhongduPower? zhongdu = player.GetPower<ZhongduPower>();
			if (zhongdu?.Amount > 0)
				await PowerCmd.Apply<ZhongduPower>(choiceContext, target, zhongdu.Amount, player, this);

			// 孢子 → 灾厄
			SporePower? spore = player.GetPower<SporePower>();
			if (spore?.Amount > 0)
				await PowerCmd.Apply<DoomPower>(choiceContext, target, spore.Amount, player, this);

			// 炎热 → 灾厄
			HeatPower? heat = player.GetPower<HeatPower>();
			if (heat?.Amount > 0)
				await PowerCmd.Apply<DoomPower>(choiceContext, target, heat.Amount, player, this);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级改变目标类型为全体敌人（见 TargetType 属性）
	}
}
