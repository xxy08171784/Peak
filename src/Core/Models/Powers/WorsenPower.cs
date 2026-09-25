using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

/// <summary>
/// 恶化：
///   1) 玩家失去生命时，该玩家获得 3 层灾厄（DoomPower）。
///   2) Boss 被攻击时，按恶化层数向攻击者的抽牌堆洗入等量「眩晕」(Dazed)。
///
/// 层数用 Counter 叠加（每次强化 +1），所以第 2 条其实等价于"每次被打洗 = 层数 张"。
/// 逐张造牌 + 预览的写法参考原版「人体蜂房」<see cref="PersonalHivePower"/>。
/// </summary>
public sealed class WorsenPower : PowerModel
{
    private const decimal DoomOnHpLoss = 3m;

    public override PowerType Type => PowerType.Buff;

    /// <summary>
    /// Counter 才能叠加：宾邦的强化招式每次都会 +1 层，
    /// 层数同时决定"每次被打洗入多少张眩晕"。
    /// </summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>悬停时能查看「眩晕」这张牌（同原版人体蜂房）。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromCard<Dazed>() };

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (result.UnblockedDamage <= 0)
        {
            return;
        }

        // 1) 玩家失去生命 → 获得 3 层灾厄
        if (target != base.Owner && target.Player != null)
        {
            Flash();
            await PowerCmd.Apply<DoomPower>(choiceContext, target, DoomOnHpLoss, base.Owner, null);
            return;
        }

        // 2) Boss 被攻击 → 给攻击者洗入「层数」张眩晕
        if (target == base.Owner && props.IsPoweredAttack())
        {
            Player? attacker = dealer?.Player;
            var combatState = base.Owner.CombatState;
            if (attacker == null || combatState == null || base.Amount <= 0)
            {
                return;
            }

            Flash();

            // 逐张造牌洗入抽牌堆，最后一次性预览（参考原版人体蜂房）
            CardPileAddResult[] statusCards = new CardPileAddResult[base.Amount];
            for (int i = 0; i < base.Amount; i++)
            {
                CardModel daze = combatState.CreateCard<Dazed>(attacker);
                statusCards[i] = await CardPileCmd.AddGeneratedCardToCombat(daze, PileType.Draw, attacker, CardPilePosition.Random);
            }

            CardCmd.PreviewCardPileAdd(statusCards);
            await Cmd.Wait(0.5f);
        }
    }
}
