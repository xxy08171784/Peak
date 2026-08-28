using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

/// <summary>
/// 夜深：1 费能力卡，罕见。
/// 升级前：获得能力，在你的回合结束时，给予所有敌人 1 层寒冷。
/// 升级后：立即给予所有敌人 1 层寒冷。获得能力，在你的回合结束时，给予所有敌人 1 层寒冷。
/// </summary>
public sealed class LateAtNight : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ColdPower>(),
        HoverTipFactory.FromPower<LateAtNightPower>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ColdPower>(1m)
    };

    public LateAtNight()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 播放能力强化动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

        decimal coldAmount = base.DynamicVars["ColdPower"].BaseValue;

        // 赋予玩家 LateAtNightPower 能力（回合结束时施加寒冷）
        await PowerCmd.Apply<LateAtNightPower>(
            choiceContext,
            base.Owner.Creature,
            coldAmount,
            base.Owner.Creature,
            this
        );

        // 升级后：立即给予所有敌人一层寒冷
        if (base.IsUpgraded)
        {
            var combatState = base.Owner.Creature.CombatState;
            if (combatState != null)
            {
                foreach (var enemy in combatState.HittableEnemies)
                {
                    await PowerCmd.Apply<ColdPower>(choiceContext, enemy, coldAmount, base.Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后：打出时立即施放一次寒冷（在 OnPlay 中处理）
    }
}