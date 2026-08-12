using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山专家：Scout 的强化版初始遗物。
/// 战斗开始时获得 1 能量 + 2 层覆甲，然后切换到 0 海岛场景，
/// 之后每回合开始时场景 +1（0123 循环），触发对应效果。
/// 继承自 MyClimbing，在场景循环的基础上增加了初始资源。
/// </summary>
public sealed class MountaineeringExpert : MyClimbing
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature == null)
		{
			return;
		}

		// 先执行基类的场景初始化（切到 0 海岛 + 触发海岛效果）
		await base.BeforeCombatStart();

		// 额外：获得 1 能量
		await PlayerCmd.GainEnergy(1m, base.Owner);

		// 额外：获得 2 层覆甲
		var choiceContext = new ThrowingPlayerChoiceContext();
		await PowerCmd.Apply<PlatingPower>(
			choiceContext,
			base.Owner.Creature,
			2m,
			base.Owner.Creature,
			null
		);

		Flash();
	}
}
