namespace peak.Core.Models.Cards;

/// <summary>
/// 食物卡标记接口。
/// 由于原版 CardTag/CardKeyword 是编译期固定枚举（无法通过 mod 扩展"食物"标签），
/// 这里用接口来标识"食物"牌。需要识别食物牌的机制（如"每当你打出食物牌时"）用
/// <c>card is IFoodCard</c> 判断即可。
/// </summary>
public interface IFoodCard
{
}
