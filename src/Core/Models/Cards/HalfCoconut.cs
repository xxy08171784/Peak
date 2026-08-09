using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using peak.Core.Models.Powers;

namespace peak.Core.Models.Cards;

public sealed class HalfCoconut : CardModel,IFoodCard
{
    // 告诉系统此卡牌获得护甲（用于 Osty 自动目标等系统逻辑）
    public override bool GainsBlock => true;

    // 关键字：消耗（Exhaust）
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

    // 基础变量：4点格挡，带 ValueProp.Move 动画/属性
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new BlockVar(4m, ValueProp.Move) };

    public HalfCoconut()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }

    // 100% 精准的格挡触发逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        // 2. 减少自身最多 5 点炎热值（HeatPower）
        HeatPower? heatPower = base.Owner.Creature.GetPower<HeatPower>();
        if (heatPower != null && heatPower.Amount > 0)
        {
            // 计算实际能扣减的层数（最多 5 层，防止层数不足 5 时算成负数）
            int reduceAmount = Math.Min(heatPower.Amount, 5);

            // 调用 PowerCmd 减少炎热值
            await PowerCmd.ModifyAmount(choiceContext, heatPower, -reduceAmount, null, null);
        }
    }

    // 升级逻辑：格挡 4 -> 6 (+2)
    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
    }

    /// <summary>
    /// 静态生成函数：向玩家手牌中加入指定数量的【半块椰子】
    /// </summary>
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, bool isUpgraded, ICombatState combatState)
    {
        if (count == 0 || CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }

        List<CardModel> coconuts = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            HalfCoconut coconut = combatState.CreateCard<HalfCoconut>(owner);
            if (isUpgraded)
            {
                coconut.UpgradeInternal();
            }
            coconuts.Add(coconut);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(coconuts, PileType.Hand, owner);
        return coconuts;
    }
}