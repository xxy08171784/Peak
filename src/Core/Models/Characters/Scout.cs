using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

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

    public override List<string> GetArchitectAttackVfx()
    {
        throw new System.NotImplementedException();
    }

    public override Color NameColor => new Color("#FFD700");

    public override int StartingHp => 75;

    public override int StartingGold => 99;

    public override CardPoolModel CardPool => ModelDb.CardPool<ScoutCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ScoutPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ScoutRelicPool>();

    public override List<CardModel> StartingDeck => [
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
    ];

    public override List<RelicModel> StartingRelics => [
        ModelDb.Relic<MyClimbing>()
    ];

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.25f;

    public override Color EnergyLabelOutlineColor => Colors.Purple;

    public override Color DialogueColor => Colors.Purple;

    public override Color MapDrawingColor => Colors.Purple;

    public override Color RemoteTargetingLineColor => Colors.Purple;

    public override Color RemoteTargetingLineOutline => Colors.Purple;
}