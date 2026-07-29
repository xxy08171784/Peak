using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;

using peak.Core.Models.CardPools;
using peak.Core.Models.Cards;
using peak.Core.Models.PotionPools;
using peak.Core.Models.Relics;
using peak.Core.Models.RelicsPools;

namespace peak.Core.Models.Characters;

public sealed class Scout : CharacterModel
{
	public const string energyColorName = "scout";

	public override CharacterGender Gender => CharacterGender.Masculine;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override Color NameColor => new Color("FFD700"); // Gold / Yellow

	public override int StartingHp => 75;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<ScoutCardPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<ScoutPotionPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<ScoutRelicPool>();

	// 初始卡组
	public override IEnumerable<CardModel> StartingDeck => new CardModel[10]
	{
		ModelDb.Card<StrikeScout>(),
		ModelDb.Card<StrikeScout>(),
		ModelDb.Card<StrikeScout>(),
		ModelDb.Card<StrikeScout>(),
		ModelDb.Card<StrikeScout>(),
		ModelDb.Card<DefendScout>(),
		ModelDb.Card<DefendScout>(),
		ModelDb.Card<DefendScout>(),
		ModelDb.Card<DefendScout>(),
		ModelDb.Card<MixedNuts>()
	};

	// 初始遗物
	public override IReadOnlyList<RelicModel> StartingRelics => new RelicModel[1]
	{
		ModelDb.Relic<MyClimbing>()
	};

	// =====================================================================
	// 重写基类中被允许 override 的虚属性（其余非虚属性交由 Harmony 补丁拦截重写）
	// =====================================================================

	// 重写按钮图标路径（指向铁甲兵图标）
	protected override string CharacterSelectIconPath => "res://assets/packed/character_select/char_select_ironclad.png";

	// 重写按钮未解锁时的图标路径
	protected override string CharacterSelectLockedIconPath => "res://assets/packed/character_select/char_select_ironclad_locked.png";
	// =====================================================================

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => Colors.Purple;
	public override Color MapDrawingColor => Colors.Purple;
	public override Color RemoteTargetingLineColor => Colors.Purple;
	public override Color RemoteTargetingLineOutline => Colors.Purple;
	public override List<string> GetArchitectAttackVfx()
	{
		int num = 5;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_attack_blunt";
		num2++;
		span[num2] = "vfx/vfx_heavy_blunt";
		num2++;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_bloody_impact";
		num2++;
		span[num2] = "vfx/vfx_rock_shatter";
		return list;
	}
	public override string CharacterSelectSfx =>
		ModelDb.Character<Ironclad>().CharacterSelectSfx;

	public override string CharacterTransitionSfx =>
		"event:/sfx/ui/wipe_ironclad";


	public override NCreatureVisuals CreateCustomVisuals()
{
	return NodeFactory<NCreatureVisuals>.CreateFromScene("res://scenes/creature_visuals/scout.tscn");
}




	
}
