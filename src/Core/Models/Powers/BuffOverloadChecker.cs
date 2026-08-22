using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 自动选目标 Selector（复刻低语耳环的 VakuuCardSelector：按顺序取前 maxSelect 张）。
/// </summary>
public sealed class ScoutAutoPlaySelector : ICardSelector
{
    public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
    {
        return Task.FromResult((IEnumerable<CardModel>)options.Take(maxSelect).ToList());
    }

    public CardRewardSelection GetSelectedCardReward(
        IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> alternatives)
    {
        return new CardRewardSelection
        {
            card = options.FirstOrDefault()?.Card
        };
    }
}

/// <summary>
/// 三 buff（炎热/孢子/中毒）总和 ≥ 100 的"过载"判定器。
///
/// 触发时机：三个 power 的 AfterPowerAmountChanged(amount > 0) 时调用（只有"获得"才判定，
/// "失去"走 amount &lt; 0 不会再次判定 → 天然免疫死循环）。
///
/// 判定优先级（并列时）：炎热 &gt; 中毒 &gt; 孢子。
/// - 炎热最多：循环 3 次 { 失去 7 炎热（触发被动打敌人 7 伤=加强），自伤 1 }
/// - 中毒最多：获得 虚弱1/易伤1/脆弱1，失去 20 中毒
/// - 孢子最多：失去 30 孢子，僵尸立即接管本回合（低语耳环式自动打牌）
///
/// _processing 标志防重入：接管期间打出的牌若再给 Owner 加 buff，不会递归触发。
/// 分支执行完（含接管结束）后会【循环重查】：只要总和仍 ≥ 100 就继续判定，
/// 因此接管过程中"错过"的判定会在接管结束后被兜底触发，直到总和 < 100 或达防御上限。
/// </summary>
public static class BuffOverloadChecker
{
    private const int Cap = 100;
    private const int HeatCostPerTick = 7;   // 炎热分支每轮失去的炎热
    private const int HeatTicks = 3;         // 炎热分支轮数
    private const int SporeCost = 30;        // 孢子分支失去的孢子
    private const int PoisonCost = 20;       // 中毒分支失去的中毒
    private const int MaxAutoPlayedCards = 13; // 接管时最多自动打出的牌数（同低语耳环）
    private const int MaxChainPerCall = 6;   // 单次触发链的最大判定次数（防御性上限，防极端情况无限接管）

    private static bool _processing;

    public static bool IsProcessing => _processing;

    public static async Task TryTrigger(Creature owner, PlayerChoiceContext choiceContext)
    {
        // 防重入：接管循环中打出的牌若再给 Owner 加 buff，不会递归触发
        // （这些"错过"的判定会在分支执行完后的循环重查中被兜底处理）
        if (_processing)
        {
            return;
        }
        if (!CombatManager.Instance.IsInProgress)
        {
            return;
        }

        _processing = true;
        try
        {
            // 循环重查：每执行完一个分支，只要总和仍 ≥ 100 就继续判定（链式过载）
            int chain = 0;
            while (chain < MaxChainPerCall && CombatManager.Instance.IsInProgress && owner.IsAlive)
            {
                int heat = owner.GetPowerAmount<HeatPower>();
                int spore = owner.GetPowerAmount<SporePower>();
                int poison = owner.GetPowerAmount<ZhongduPower>();
                int total = heat + spore + poison;
                if (total < Cap)
                {
                    break;
                }

                chain++;
                GD.Print($"[BuffOverload] 判定链第 {chain} 次：heat={heat} spore={spore} poison={poison} total={total}");

                if (heat >= poison && heat >= spore)
                {
                    // 炎热最多（并列时炎热优先）
                    await HeatBranch(owner, choiceContext);
                }
                else if (poison >= spore)
                {
                    // 中毒次之（炎热不占优时）
                    await PoisonBranch(owner, choiceContext);
                }
                else
                {
                    // 孢子最多
                    await SporeBranch(owner, choiceContext);
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[BuffOverload] ERROR: {e}");
        }
        finally
        {
            _processing = false;
        }
    }

    /// <summary>🔥 炎热最多：循环 3 次 { 失去 7 炎热（触发被动打敌人 7 伤），自伤 1 }</summary>
    private static async Task HeatBranch(Creature owner, PlayerChoiceContext choiceContext)
    {
        GD.Print("[BuffOverload] 炎热过载：失去 7 炎热 ×3，自伤 1 ×3");
        for (int i = 0; i < HeatTicks; i++)
        {
            if (!owner.IsAlive || CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }
            // 失去 7 炎热 → 触发 HeatPower 被动（对随机敌人造成 7 伤，纵火高手则 AOE 翻倍）= 加强
            await PowerCmd.Apply<HeatPower>(choiceContext, owner, -HeatCostPerTick, owner, null);
            // 对自己造成 1 点可格挡伤害（ValueProp.Move = 可被格挡）
            await CreatureCmd.Damage(choiceContext, owner, 1m, ValueProp.Move, null, null);
            if (i < HeatTicks - 1)
            {
                await Cmd.CustomScaledWait(0.1f, 0.25f);
            }
        }
    }

    /// <summary>🟣 中毒最多：获得 虚弱1/易伤1/脆弱1，失去 20 中毒</summary>
    private static async Task PoisonBranch(Creature owner, PlayerChoiceContext choiceContext)
    {
        GD.Print("[BuffOverload] 中毒过载：获得 虚弱/易伤/脆弱 各 1，失去 20 中毒");
        await PowerCmd.Apply<WeakPower>(choiceContext, owner, 1m, owner, null);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, owner, 1m, owner, null);
        await PowerCmd.Apply<FrailPower>(choiceContext, owner, 1m, owner, null);
        await PowerCmd.Apply<ZhongduPower>(choiceContext, owner, -PoisonCost, owner, null);
    }

    /// <summary>🟢 孢子最多：失去 30 孢子，僵尸立即接管本回合（低语耳环式自动打牌）</summary>
    private static async Task SporeBranch(Creature owner, PlayerChoiceContext choiceContext)
    {
        GD.Print("[BuffOverload] 孢子过载：失去 30 孢子，僵尸接管本回合");
        await PowerCmd.Apply<SporePower>(choiceContext, owner, -SporeCost, owner, null);

        Player? player = owner.Player;
        if (player == null || !player.Creature.IsAlive || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        await AutoPlayTurn(player, choiceContext);
    }

    /// <summary>
    /// 接管：复刻低语耳环的自动打牌循环。
    /// 只在本地玩家机器上执行（LocalContext.IsMe 守卫），消除联机锁步分叉风险。
    /// 循环安全网：战斗结束 / 玩家已结束回合 / 手牌打空 / 无牌可打 都会停止。
    /// </summary>
    private static async Task AutoPlayTurn(Player player, PlayerChoiceContext choiceContext)
    {
        // 只在本地玩家机器上执行自动打牌，联机时远端客户端通过同步系统获知结果
        if (!LocalContext.IsMe(player.Creature))
        {
            return;
        }
        Creature owner = player.Creature;
        ICombatState? combatState = owner.CombatState;
        int cardsPlayed = 0;
        using (CardSelectCmd.PushSelector(new ScoutAutoPlaySelector()))
        {
            for (; cardsPlayed < MaxAutoPlayedCards; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    break;
                }
                if (CombatManager.Instance.IsPlayerReadyToEndTurn(player))
                {
                    break;
                }
                CardPile pile = PileType.Hand.GetPile(player);
                CardModel? card = pile.Cards.FirstOrDefault(c => c.CanPlay());
                if (card == null)
                {
                    break;
                }
                Creature? target = GetTarget(card, owner, combatState);
                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }
        }
        if (cardsPlayed > 0)
        {
            GD.Print($"[BuffOverload] 僵尸接管结束：自动打出 {cardsPlayed} 张牌");
        }
    }

    /// <summary>选目标：敌人取最左，队友取第一个活着的其他玩家，自身类取自己。</summary>
    private static Creature? GetTarget(CardModel card, Creature owner, ICombatState? combatState)
    {
        if (combatState == null)
        {
            return null;
        }
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => combatState.Allies.FirstOrDefault(c => c != null && c.IsAlive && c.IsPlayer && c != owner),
            TargetType.AnyPlayer => owner,
            _ => null,
        };
    }
}
