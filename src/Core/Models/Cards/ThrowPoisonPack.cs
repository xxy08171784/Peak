using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace peak.Core.Models.Cards;

public sealed class ThrowPoisonPack : CardModel
{
    // 1. 悬浮提示：显示中毒说明
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<PoisonPower>()
    };

    // 2. 动态变量：中毒属于官方原生 Power，使用 PowerVar<PoisonPower> 声明 6 点基础中毒
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(6m)
    };

    // 3. 构造函数：2 费，技能卡，罕见，目标为所有敌人 (TargetType.AllEnemies)
    public ThrowPoisonPack()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    // 4. 打出逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 播放玩家施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 获取中毒数值（原生 Poison 变量可直接用 base.DynamicVars.Poison.BaseValue 访问）
        decimal poisonAmount = base.DynamicVars.Poison.BaseValue;

        // 获取当前战斗中所有可被击中的敌人
        var enemies = base.CombatState.HittableEnemies;

        // 遍历所有敌人并施加中毒
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<PoisonPower>(
                choiceContext,
                enemy,
                poisonAmount,
                base.Owner.Creature,
                this
            );
        }
    }

    // 5. 升级逻辑：中毒层数 +2 (从 6 变为 8)
    protected override void OnUpgrade()
    {
        base.DynamicVars.Poison.UpgradeValueBy(2m);
    }
}