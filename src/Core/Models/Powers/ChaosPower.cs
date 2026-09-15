using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 混沌：每打出 3 张牌，触发一次随机效果。
/// 计数写法参考原版计数类遗物/power（CalamityPower / Nunchaku 的 AfterCardPlayed + 取模），
/// **每个玩家各自计数**（多人在各端一致），随机用共享 RNG。
///
/// 9 种随机效果：
///   1 强制弃 1 张手牌 / 2 失去 3 点生命上限 / 3 获得 20 孢子 / 4 失去 20 炎热 /
///   5 获得 4 中毒 / 6 失去 1 费 / 7 受到 5 点伤害 / 8 Boss 获得 1 点荆棘 / 9 一张随机手牌附加消耗
/// </summary>
public sealed class ChaosPower : PowerModel
{
    private const int CardsPerTrigger = 3;

    private const int EffectCount = 9;

    private class Data
    {
        public readonly Dictionary<Creature, int> CountsByPlayer = new Dictionary<Creature, int>();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    protected override object InitInternalData() => new Data();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? player = cardPlay.Card.Owner;
        Creature? playerCreature = player?.Creature;
        if (player == null || playerCreature == null || playerCreature == base.Owner)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.CountsByPlayer.TryGetValue(playerCreature, out int count);
        count++;
        data.CountsByPlayer[playerCreature] = count;

        if (count % CardsPerTrigger != 0)
        {
            return;
        }

        Flash();
        await TriggerRandomEffect(player, choiceContext);
    }

    /// <summary>Boss 的"强化"招式：连续触发 <paramref name="times"/> 次随机效果（每次随机一名存活玩家）。</summary>
    public async Task TriggerRandomEffects(int times)
    {
        // 敌方回合内触发，可能弹出"强制弃牌"选择：用 BlockingPlayerChoiceContext
        // （原版文档指定的敌方回合玩家选择上下文），不能用 ThrowingPlayerChoiceContext。
        var ctx = new BlockingPlayerChoiceContext();

        for (int i = 0; i < times; i++)
        {
            var players = PlayerList();
            if (players.Count == 0)
            {
                return;
            }

            var rng = players[0].RunState.Rng.CombatCardGeneration;
            Player target = players[rng.NextInt(0, players.Count)];
            await TriggerRandomEffect(target, ctx);
        }
    }

    private List<Player> PlayerList()
    {
        var list = new List<Player>();
        var combatState = base.Owner.CombatState;
        if (combatState == null)
        {
            return list;
        }

        foreach (Creature creature in combatState.PlayerCreatures)
        {
            if (creature.IsAlive && creature.Player != null)
            {
                list.Add(creature.Player);
            }
        }

        return list;
    }

    private async Task TriggerRandomEffect(Player player, PlayerChoiceContext ctx)
    {
        Creature playerCreature = player.Creature;
        var rng = player.RunState.Rng.CombatCardGeneration;

        switch (rng.NextInt(0, EffectCount))
        {
            case 0: // 强制弃 1 张手牌
            {
                CardModel? card = (await CardSelectCmd.FromHandForDiscard(
                    context: ctx,
                    player: player,
                    prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
                    filter: null,
                    source: this)).FirstOrDefault();

                if (card != null)
                {
                    await CardCmd.Discard(ctx, card);
                }

                break;
            }

            case 1: // 失去 3 点生命上限
                await CreatureCmd.LoseMaxHp(ctx, playerCreature, 3m, false);
                break;

            case 2: // 获得 20 孢子
                await PowerCmd.Apply<SporePower>(ctx, playerCreature, 20m, base.Owner, null);
                break;

            case 3: // 失去 20 炎热
            {
                HeatPower? heat = playerCreature.GetPower<HeatPower>();
                if (heat != null && heat.Amount > 0)
                {
                    int reduce = System.Math.Min(heat.Amount, 20);
                    await PowerCmd.ModifyAmount(ctx, heat, -reduce, base.Owner, null);
                }

                break;
            }

            case 4: // 获得 4 中毒
                await PowerCmd.Apply<ZhongduPower>(ctx, playerCreature, 4m, base.Owner, null);
                break;

            case 5: // 失去 1 费
                await PlayerCmd.LoseEnergy(1m, player);
                break;

            case 6: // 受到 5 点伤害（不可格挡）
                await CreatureCmd.Damage(ctx, playerCreature, 5m,
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, base.Owner, null, null);
                break;

            case 7: // Boss 获得 1 点荆棘
                await PowerCmd.Apply<ThornsPower>(ctx, base.Owner, 1m, base.Owner, null);
                break;

            default: // 给一张随机手牌附加"消耗"
            {
                var hand = PileType.Hand.GetPile(player).Cards;
                List<CardModel> candidates = hand.Where(c => !c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
                if (candidates.Count > 0)
                {
                    CardModel pick = candidates[rng.NextInt(0, candidates.Count)];
                    CardCmd.ApplyKeyword(pick, CardKeyword.Exhaust);
                }

                break;
            }
        }
    }
}
