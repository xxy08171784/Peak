using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
namespace peak.Core.Models.Cards
{
    public sealed class RockBoltCard : CardModel
    {
        // 定义卡牌所需的动态变量
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[2]
        {
            new PowerVar<PlatingPower>(2m), // 获得2层覆甲
            new CardsVar(1) // 基础抽1张牌
        };

        // 悬停提示：显示“覆甲”的机制说明
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1]
        {
            HoverTipFactory.FromPower<PlatingPower>()
        };

        // 构造函数：1费，技能牌，普通稀有度，目标为自己
        public RockBoltCard()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        // 卡牌打出时的逻辑
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            // 1. 施加覆甲效果
            await PowerCmd.Apply<PlatingPower>(
                choiceContext,
                base.Owner.Creature,
                base.DynamicVars["PlatingPower"].BaseValue,
                base.Owner.Creature,
                this
            );

            // 2. 执行抽牌效果
            await CardPileCmd.Draw(
                choiceContext,
                base.DynamicVars.Cards.BaseValue,
                base.Owner
            );
        }

        // 卡牌升级逻辑
        protected override void OnUpgrade()
        {
            // 升级后，抽牌数量增加1张（从1张变为2张）
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}