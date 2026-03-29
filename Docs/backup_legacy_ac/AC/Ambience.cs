using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_ambience.html")]
	public class Ambience : Soundtrack
	{
		protected override List<MusicStorage> Storages
		{
			get
			{
				return KickStarter.settingsManager.ambienceStorages;
			}
		}

		protected new void Awake()
		{
			soundType = SoundType.SFX;
			playWhilePaused = false;
			base.Awake();
		}

		public override MainData SaveMainData(MainData mainData)
		{
			mainData.lastAmbienceQueueData = CreateLastSoundtrackString();
			mainData.ambienceQueueData = CreateTimesampleString();
			mainData.ambienceTimeSamples = 0;
			mainData.lastAmbienceTimeSamples = base.LastTimeSamples;
			if (GetCurrentTrackID() >= 0)
			{
				MusicStorage soundtrack = GetSoundtrack(GetCurrentTrackID());
				if (soundtrack != null && soundtrack.audioClip != null && base.audioSource.clip == soundtrack.audioClip && IsPlaying())
				{
					mainData.ambienceTimeSamples = base.audioSource.timeSamples;
				}
			}
			mainData.oldAmbienceTimeSamples = CreateOldTimesampleString();
			return mainData;
		}

		public override void LoadMainData(MainData mainData)
		{
			LoadMainData(mainData.ambienceTimeSamples, mainData.oldAmbienceTimeSamples, mainData.lastAmbienceTimeSamples, mainData.lastAmbienceQueueData, mainData.ambienceQueueData);
		}
	}
}
