using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Cards;

public sealed class DefendScout : CardModel
{
	// 声明该卡牌能够获得格挡，便于游戏内部机制进行识别
	public override bool GainsBlock => true;

	// 标记卡牌拥有“防御”（Defend）标签，便于与特定遗物或机制联动
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };

	// 设定卡牌的初始属性，这里定义了基础格挡值为 5 点
	protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new BlockVar(5m, ValueProp.Move) };

	public DefendScout()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
		// 1 费，技能牌，基础稀有度，目标为自身
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 执行获得格挡的指令。
		// 参数依次为：获得格挡的生物（玩家自己）、格挡数值对象、以及出牌上下文
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
	}

	protected override void OnUpgrade()
	{
		// 升级后格挡值增加 3 点（总格挡值变为 8 点）
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
