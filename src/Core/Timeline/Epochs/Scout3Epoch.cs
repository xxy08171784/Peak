using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using peak.Core.Models.Relics;

namespace peak.Core.Timeline.Epochs;

/// <summary>
/// 第三章-重大事故：宾邦航班坠毁，尖塔耸立。
/// 击败 Act2 Boss 后解锁（对应 Silent3Epoch 解锁模式）。
/// 解锁 3 个童子军遗物。
/// </summary>
public class Scout3Epoch : EpochModel
{
	public override string Id => "SCOUT3_EPOCH";
	public override EpochEra Era => EpochEra.Flourish1;
	public override int EraPosition => 6;
	public override string StoryId => "Scout";

	public static List<RelicModel> Relics
	{
		get
		{
			int num = 3;
			List<RelicModel> list = new List<RelicModel>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<RelicModel> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = ModelDb.Relic<Cactus>();
			num2++;
			span[num2] = ModelDb.Relic<GlowStick>();
			num2++;
			span[num2] = ModelDb.Relic<ColdStorage>();
			return list;
		}
	}

	public override string UnlockText => CreateRelicUnlockText(Relics);

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueRelicUnlock(Relics);
	}
}
