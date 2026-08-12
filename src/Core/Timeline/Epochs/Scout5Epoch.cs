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
/// 第五章-领队的笔记：主角发现领队迈尔斯的带血字迹。
/// 击败 15 名精英后解锁（对应 Silent5Epoch 解锁模式）。
/// 解锁 3 张童子军卡牌。
/// </summary>
public class Scout5Epoch : EpochModel
{
	public override string Id => "SCOUT5_EPOCH";
	public override EpochEra Era => EpochEra.Flourish3;
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
			span[num2] = ModelDb.Card<AllIntoBackpack>();
			num2++;
			span[num2] = ModelDb.Card<SecretRecipeChiliSauce>();
			num2++;
			span[num2] = ModelDb.Card<CrushTheIce>();
			return list;
		}
	}

	public override string UnlockText => CreateCardUnlockText(Cards);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCardUnlock(Cards);
	}
}
