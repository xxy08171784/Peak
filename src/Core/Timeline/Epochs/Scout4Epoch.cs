using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using peak.Core.Models.Potions;

namespace peak.Core.Timeline.Epochs;

/// <summary>
/// 第四章-寻找朋友：主角担心失踪的伙伴们。
/// 击败 Act3 Boss 后解锁（对应 Silent4Epoch 解锁模式）。
/// 解锁 3 个童子军药水。
/// </summary>
public class Scout4Epoch : EpochModel
{
	public override string Id => "SCOUT4_EPOCH";
	public override EpochEra Era => EpochEra.Flourish2;
	public override int EraPosition => 6;
	public override string StoryId => "Scout";

	public static List<PotionModel> Potions
	{
		get
		{
			int num = 3;
			List<PotionModel> list = new List<PotionModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<PotionModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Potion<PoisonedBottle>();
			num2++;
			span[num2] = ModelDb.Potion<SpritePotion>();
			num2++;
			span[num2] = ModelDb.Potion<MilkPotion>();
			return list;
		}
	}

	public override string UnlockText => CreatePotionUnlockText(Potions);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueuePotionUnlock(Potions);
		LocString locString = new LocString("epochs", Id + ".unlock");
		NTimelineScreen.Instance.QueueMiscUnlock(locString.GetFormattedText() ?? "");
	}
}
