using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 炸药：对所有敌人造成 10（13）点伤害，对拥有[gold]渐冻[/gold]的敌人造成双倍伤害。
/// 2 费，攻击牌，普通稀有度，目标所有敌人。
/// </summary>
public sealed class Explosive : CardModel
{
	// 卡面图片（文件名与卡牌 ID 一致：explosive.png）
	public override string PortraitPath => ImageHelper.GetImagePath("packed/card_portraits/scout/explosive.png");

	// 基础变量：10 点伤害（升级后 13 点）
	protected override IEnumerable<DynamicVar> CanonicalVars => new[]
	{
		new DamageVar(10m, ValueProp.Move)
	};

	public Explosive()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 快照敌人列表，避免 AOE 过程中敌人死亡导致"集合已修改"异常
		List<Creature> enemies = base.CombatState.Enemies.ToList();
		foreach (Creature enemy in enemies)
		{
			// 跳过已死亡的敌人
			if (!enemy.IsAlive)
				continue;

			// 渐冻层数 > 0 视为拥有渐冻
			int frostbiteStacks = enemy.GetPower<FrostbitePower>()?.Amount ?? 0;
			decimal damage = base.DynamicVars.Damage.BaseValue;
			if (frostbiteStacks > 0)
			{
				damage *= 2m;
			}

			await DamageCmd.Attack(damage)
				.FromCard(this)
				.Targeting(enemy)
				.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后伤害 10 -> 13 (+3)
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
