using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using peak.Core.Models.Cards;

namespace peak.Core.Timeline.Epochs;

/// <summary>
/// 第二章-三人帮：主角结识猫猫和先锋，组成最佳三人帮。
/// 击败 Act1 Boss 后解锁（对应 Silent2Epoch 解锁模式）。
/// 解锁 3 张童子军卡牌。
/// </summary>
public class Scout2Epoch : EpochModel
{
	public override string Id => "SCOUT2_EPOCH";
	public override EpochEra Era => EpochEra.Invitation2;
	public override int EraPosition => 6;
	public override string StoryId => "Scout";

	public static List<CardModel> Cards
	{
		get
		{
			int num = 3;
			List<CardModel> list = new List<CardModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<CardModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Card<ChiliPepper>();
			num2++;
			span[num2] = ModelDb.Card<IceDew>();
			num2++;
			span[num2] = ModelDb.Card<Aftertaste>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
