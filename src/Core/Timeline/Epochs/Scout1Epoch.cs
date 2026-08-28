using MegaCrit.Sts2.Core.Timeline;

namespace peak.Core.Timeline.Epochs;

/// <summary>
/// 第一章-童军队员：主角加入童子军，领队传授 0 号守则。
/// 不再自动解锁角色 Scout（角色默认可用），而是在完成一场 Scout 游戏后通过
/// PostRunScout1UnlockPatch 补丁获得此 epoch。
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
		EpochModel.QueueTimelineExpansion(GetTimelineExpansion());
	}
}
