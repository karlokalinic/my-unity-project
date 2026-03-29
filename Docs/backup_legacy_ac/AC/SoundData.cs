using System;

namespace AC
{
	[Serializable]
	public class SoundData : RememberData
	{
		public bool isPlaying;

		public bool isLooping;

		public int samplePoint;

		public string clipID;

		public float relativeVolume;

		public float maxVolume;

		public float smoothVolume;

		public float fadeTime;

		public float originalFadeTime;

		public int fadeType;

		public float otherVolume;

		public float targetRelativeVolume;

		public float originalRelativeVolume;

		public float relativeChangeTime;

		public float originalRelativeChangeTime;
	}
}
