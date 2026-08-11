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
/// 获得能力：每回合开始时，给所有人（升级后仅敌人）施加 1 层寒冷。
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

        // 赋予玩家 LateAtNightPower 能力
        var power = await PowerCmd.Apply<LateAtNightPower>(
            choiceContext,
            base.Owner.Creature,
            coldAmount,
            base.Owner.Creature,
            this
        );

        // 如果卡牌已升级，将 OnlyEnemies 标志设为 1
        if (power != null && base.IsUpgraded)
        {
            power.DynamicVars[LateAtNightPower.OnlyEnemiesKey].BaseValue = 1m;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级改变了所赋予能力的逻辑范围
    }
}