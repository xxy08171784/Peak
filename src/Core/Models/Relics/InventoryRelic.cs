using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace peak.Core.Models.Relics;

/// <summary>
/// 物品栏：拾取时，选择卡组中的一张牌，添加固有词条。
/// 稀有稀有度。
/// </summary>
public sealed class InventoryRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	// 图标使用 inventory.png（与图片资源命名对应）
	protected override string IconBaseName => "inventory";

	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		if (base.Owner == null)
		{
			return;
		}

		// 从卡组中选择两张可添加固有的牌
		var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 2);
		IEnumerable<CardModel> selected = await CardSelectCmd.FromDeckGeneric(
			base.Owner,
			prefs,
			filter: c => !c.Keywords.Contains(CardKeyword.Innate));

		foreach (CardModel target in selected)
		{
			Flash();
			target.AddKeyword(CardKeyword.Innate);
		}
	}
}
