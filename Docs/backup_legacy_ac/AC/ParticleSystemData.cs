using System;

namespace AC
{
	[Serializable]
	public class ParticleSystemData : RememberData
	{
		public bool isPaused;

		public bool isPlaying;

		public float currentTime;
	}
}
