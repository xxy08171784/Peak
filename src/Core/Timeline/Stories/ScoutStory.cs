using MegaCrit.Sts2.Core.Timeline;
using peak.Core.Timeline.Epochs;

namespace peak.Core.Timeline.Stories;

public sealed class ScoutStory : StoryModel
{
	protected override string Id => "SCOUT";

	public override EpochModel[] Epochs => new EpochModel[6]
	{
		EpochModel.Get<Scout1Epoch>(),
		EpochModel.Get<Scout2Epoch>(),
		EpochModel.Get<Scout3Epoch>(),
		EpochModel.Get<Scout4Epoch>(),
		EpochModel.Get<Scout5Epoch>(),
		EpochModel.Get<Scout6Epoch>()
	};
}
