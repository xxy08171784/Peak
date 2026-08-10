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

public sealed class Bask : CardModel
{
    private const string HeatKey = "Heat";
    private const string BonusHeatKey = "BonusHeat";

    // 1. 条件高亮：玩家身上炎热层数 >= 10 时，卡牌在手牌中发金光
    protected override bool ShouldGlowGoldInternal => 
        (base.Owner?.Creature?.GetPower<HeatPower>()?.Amount ?? 0m) >= 10m;

    // 2. 悬浮提示：使用标准 C# 数组写法，替换掉反编译的编译器合成类
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<HeatPower>()
    };

    // 3. 动态变量：定义基础炎热 10，以及加成比例 5
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(HeatKey, 10m),
        new DynamicVar(BonusHeatKey, 5m)
    };

    // 4. 构造函数：1费，技能卡，罕见，目标为自己
    public Bask()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // 5. 打出逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 播放角色施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 使用标准的字符串索引器 base.DynamicVars["Key"] 读取 BaseValue
        decimal baseHeat = base.DynamicVars[HeatKey].BaseValue;
        decimal bonusRate = base.DynamicVars[BonusHeatKey].BaseValue;

        // 获取玩家当前的炎热层数
        decimal currentHeat = base.Owner.Creature.GetPower<HeatPower>()?.Amount ?? 0m;

        // 计算加成次数：每拥有 10 点炎热加成 1 次
        int bonusMultipliers = (int)(currentHeat / 10m);
        decimal totalHeatToApply = baseHeat + (bonusMultipliers * bonusRate);

        // 赋予炎热值
        await PowerCmd.Apply<HeatPower>(
            choiceContext,
            base.Owner.Creature,
            totalHeatToApply,
            base.Owner.Creature,
            this
        );
    }

    // 6. 升级逻辑：基础炎热 +5 (从 10 变为 15)
    protected override void OnUpgrade()
    {
        base.DynamicVars[HeatKey].UpgradeValueBy(5m);
    }
}