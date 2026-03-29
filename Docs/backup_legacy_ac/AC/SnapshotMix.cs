using System;
using UnityEngine.Audio;

namespace AC
{
	[Serializable]
	public class SnapshotMix
	{
		public AudioMixerSnapshot snapshot;

		public float weight;
	}
}
