using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Powers;

public sealed class RedHotPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 伤害修改钩子：每次造成伤害时动态检查
    /// </summary>
    public new decimal ModifyDamageGiven(
        ICombatState combatState,
        Creature target,
        decimal damage,
        CardModel? cardSource,
        Creature attacker)
    {
        // 1. 只有攻击卡 (Attack) 能触发双倍伤害
        if (cardSource != null && cardSource.Type == CardType.Attack)
        {
            // 2. 实时获取炎热值与当前 HP
            decimal currentHeat = Owner.GetPower<HeatPower>()?.Amount ?? 0m;
            decimal currentHp = Owner.MaxHp;

            // 3. 条件满足：炎热值 >= 当前生命值，伤害翻倍
            if (currentHeat >= currentHp)
            {
                Flash(); // 图标闪烁反馈
                return damage * 2m;
            }
        }

        return damage;
    }
}