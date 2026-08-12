using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Relics;

/// <summary>
/// 登山专家（MountaineerExpert）：我的攀登的强化版。
/// 在【我的攀登】的全部效果基础上，额外获得：
/// - 每场战斗开始时获得 1 点能量
/// - 每场战斗开始时获得 2 层覆甲
/// 由 Orobas 先古事件的【欧洛巴斯之触】把初始遗物【我的攀登】替换而来。
/// </summary>
public class MountaineerExpert : MyClimbing
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	// 战斗开始时额外获得的能量/覆甲数值（供本地化动态变量引用）
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new EnergyVar(1),
		new PowerVar<PlatingPower>(2m)
	};

	/// <summary>
	/// 战斗开始时：先执行【我的攀登】原有逻辑（切到海岛0并触发3层覆甲），
	/// 再额外获得 1 点能量和 2 层覆甲。
	/// </summary>
	public override async Task BeforeCombatStart()
	{
		await base.BeforeCombatStart();

		if (base.Owner?.Creature == null)
		{
			return;
		}

		var choiceContext = new ThrowingPlayerChoiceContext();

		// 额外效果 1：获得 1 点能量
		await PlayerCmd.GainEnergy(base.DynamicVars["Energy"].BaseValue, base.Owner);

		// 额外效果 2：获得 2 层覆甲
		await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, base.DynamicVars["PlatingPower"].BaseValue, base.Owner.Creature, null);
	}
}
