using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 纵火高手：标记性 Buff，拥有此 Power 时 HeatPower 的回合结束伤害翻倍。
/// </summary>
public sealed class ArsonExpertPower : PowerModel
{
	// 正向增益
	public override PowerType Type => PowerType.Buff;

	// 层数堆叠（多层可叠加倍率：层数+1 倍，如 1 层=2倍，2 层=3倍...）
	public override PowerStackType StackType => PowerStackType.Counter;

	// 不允许负数
	public override bool AllowNegative => false;

	/// <summary>
	/// 获取伤害倍率：层数 + 1（1层=2倍，2层=3倍...）
	/// </summary>
	public int DamageMultiplier => Amount + 1;
}