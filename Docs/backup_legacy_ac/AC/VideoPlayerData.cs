using System;

namespace AC
{
	[Serializable]
	public class VideoPlayerData : RememberData
	{
		public bool isPlaying;

		public long currentFrame;

		public double currentTime;

		public string clipAssetID;
	}
}
