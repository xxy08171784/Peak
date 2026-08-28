using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace peak.Core.Models.Powers;

public sealed class RedHotPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 每打出一张牌，失去 1 点炎热，对所有敌人造成 3 点伤害。
    /// 如果没有炎热，则不造成额外伤害。
    /// 仅触发宿主玩家自己的出牌（多人模式下其他队友打牌不触发）。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;

        // 只触发宿主玩家自己的出牌
        if (cardPlay.Card.Owner.Creature != Owner) return;

        decimal heat = Owner.GetPowerAmount<HeatPower>();
        if (heat <= 0m) return;

        Flash();
        await PowerCmd.Apply<HeatPower>(choiceContext, Owner, -1m, Owner, null);

        // 对所有敌人造成 3 点伤害
        var enemies = Owner.CombatState?.Enemies;
        if (enemies != null)
        {
            foreach (var enemy in enemies.Where(e => e.IsAlive))
            {
                await CreatureCmd.Damage(choiceContext, enemy, 3m, ValueProp.Move, null, null);
            }
        }
    }
}