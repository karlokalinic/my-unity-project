using System;

namespace AC
{
	[Serializable]
	public class TimelineData : RememberData
	{
		public bool isPlaying;

		public double currentTime;

		public string trackObjectData;

		public string timelineAssetID;
	}
}
