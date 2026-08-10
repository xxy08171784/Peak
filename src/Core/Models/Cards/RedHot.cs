using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

public sealed class RedHot : CardModel
{
    // 悬浮提示：显示 炎热值 和 红温 的能力说明
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<HeatPower>(),
        HoverTipFactory.FromPower<RedHotPower>()
    };

    // 2 费，能力卡，稀有，目标为自己
    public RedHot()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 播放 PowerUp 动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);

        // 赋予玩家 RedHotPower 能力
        await PowerCmd.Apply<RedHotPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this
        );
    }

    // 升级：费用 -1 (2 费降为 1 费，参考了 ShadowStep 的写法)
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}