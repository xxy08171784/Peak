using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using peak.Core.Models.Characters;

namespace peak.Core.Timeline.Epochs;

/// <summary>
/// 第一章-童军队员：主角加入童子军，领队传授 0 号守则。
/// 对应 Silent1Epoch —— 由 NeowEpoch.QueueUnlocks() 在首次打开时间线时获得。
/// 获得后展开 Scout2-6 的槽位。
/// </summary>
public class Scout1Epoch : EpochModel
{
	public override string Id => "SCOUT1_EPOCH";
	public override EpochEra Era => EpochEra.Invitation1;
	public override int EraPosition => 6;
	public override string StoryId => "Scout";

	public override EpochModel[] GetTimelineExpansion()
	{
		return new EpochModel[5]
		{
			EpochModel.Get(EpochModel.GetId<Scout2Epoch>()),
			EpochModel.Get(EpochModel.GetId<Scout3Epoch>()),
			EpochModel.Get(EpochModel.GetId<Scout4Epoch>()),
			EpochModel.Get(EpochModel.GetId<Scout5Epoch>()),
			EpochModel.Get(EpochModel.GetId<Scout6Epoch>())
		};
	}

	public override void QueueUnlocks()
	{
		NTimelineScreen.Instance.QueueCharacterUnlock<Scout>(this);
		SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.Character<Scout>().Id;
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
