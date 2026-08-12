namespace peak.Core.Models.Cards;

/// <summary>
/// 道具卡标记接口。
/// 用于标识"道具"牌（装备/工具类卡牌），供遗物（如工具箱）识别。
/// 与 IFoodCard 类似，使用 <c>card is IItemCard</c> 判断即可。
/// </summary>
public interface IItemCard
{
}
