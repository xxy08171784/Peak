using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 医疗箱：失去你的所有中毒，获得 16（20）点格挡。
/// 2 费，技能牌，罕见稀有度，目标自身。
/// </summary>
public sealed class MedicalKit : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：medical_kit.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/medical_kit.png");

	

	// 动态变量：基础格挡 16 点（升级后 20 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(16m, ValueProp.Move)
	};

	public MedicalKit()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 1. 失去你的所有中毒
		ZhongduPower? zhongdu = base.Owner.Creature.GetPower<ZhongduPower>();
		if (zhongdu != null && zhongdu.Amount > 0)
		{
			await PowerCmd.ModifyAmount(choiceContext, zhongdu, -zhongdu.Amount, base.Owner.Creature, this);
		}

		// 2. 获得 16（20）点格挡
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后格挡 16 -> 20 (+4)
		base.DynamicVars.Block.UpgradeValueBy(4m);
	}
}
