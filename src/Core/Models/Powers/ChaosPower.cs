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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 混沌：每打出 3 张牌，触发一次随机效果。
///
/// 计数器写法参考原版「凋萎存在」<see cref="WitheringPresencePower"/>：
/// 每个玩家一个实例（<see cref="PowerInstanceType.Instanced"/>），实例用 Target 认领归属玩家、
/// 用 DynamicVars["CardsLeft"] 倒数，并覆写 <see cref="DisplayAmount"/> 把剩余张数显示在图标上。
/// 多个实例都挂在宾邦身上，但 <see cref="PowerModel.IsVisible"/> 只让玩家看见 Target 是自己那一个，
/// 所以每人屏幕上只有一个混沌图标、显示自己的倒计时。
///
/// 9 种随机效果（数值见下面各 const）：
///   1 强制弃 1 张手牌 / 2 失去 5 点生命上限 / 3 获得 25 孢子 / 4 失去 25 炎热 /
///   5 获得 5 中毒 / 6 失去 1 费 / 7 受到 5 点伤害（可格挡）/ 8 Boss 获得 2 点荆棘 /
///   9 一张随机手牌附加消耗
/// </summary>
public sealed class ChaosPower : PowerModel
{
    /// <summary>触发间隔：每打出这么多张牌触发一次。</summary>
    public const int CardsPerTrigger = 3;

    private const int EffectCount = 9;

    private const decimal MaxHpLossAmount = 5m;
    private const decimal SporeAmount = 25m;
    private const decimal HeatLossAmount = 25m;
    private const decimal ZhongduAmount = 5m;
    private const decimal SelfDamageAmount = 5m;
    private const decimal ThornsAmount = 2m;

    private const string CardsLeftKey = "CardsLeft";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    /// <summary>每名玩家一个独立实例，各自计各自的 3 张（同原版凋萎存在）。</summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>图标上显示的数字 = 距离下次触发还差几张牌。</summary>
    public override int DisplayAmount => base.DynamicVars[CardsLeftKey].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(CardsLeftKey, CardsPerTrigger)
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只认自己这个实例归属玩家打出的牌（同原版凋萎存在）
        Player? player = cardPlay.Card.Owner;
        if (player == null || player != base.Target?.Player)
        {
            return;
        }

        base.DynamicVars[CardsLeftKey].BaseValue -= 1m;
        InvokeDisplayAmountChanged();

        if (base.DynamicVars[CardsLeftKey].IntValue > 0)
        {
            return;
        }

        Flash();
        await TriggerRandomEffect(player, choiceContext);

        base.DynamicVars[CardsLeftKey].BaseValue = CardsPerTrigger;
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// Boss 的"强化"招式：连续触发 <paramref name="times"/> 次随机效果（每次随机一名存活玩家）。
    /// 这条路径不消耗各玩家的计数器，与"每 3 张牌"是两条独立触发源。
    /// </summary>
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

            case 1: // 失去 5 点生命上限
                await CreatureCmd.LoseMaxHp(ctx, playerCreature, MaxHpLossAmount, false);
                break;

            case 2: // 获得 25 孢子
                await PowerCmd.Apply<SporePower>(ctx, playerCreature, SporeAmount, base.Owner, null);
                break;

            case 3: // 失去 25 炎热
            {
                HeatPower? heat = playerCreature.GetPower<HeatPower>();
                if (heat != null && heat.Amount > 0)
                {
                    int reduce = System.Math.Min(heat.Amount, (int)HeatLossAmount);
                    await PowerCmd.ModifyAmount(ctx, heat, -reduce, base.Owner, null);
                }

                break;
            }

            case 4: // 获得 5 中毒
                await PowerCmd.Apply<ZhongduPower>(ctx, playerCreature, ZhongduAmount, base.Owner, null);
                break;

            case 5: // 失去 1 费
                await PlayerCmd.LoseEnergy(1m, player);
                break;

            case 6: // 受到 5 点伤害（可格挡：格挡能挡下，但不吃力量/敏捷加成）
                await CreatureCmd.Damage(ctx, playerCreature, SelfDamageAmount,
                    ValueProp.Unpowered | ValueProp.Move, base.Owner, null, null);
                break;

            case 7: // Boss 获得 2 点荆棘
                await PowerCmd.Apply<ThornsPower>(ctx, base.Owner, ThornsAmount, base.Owner, null);
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
