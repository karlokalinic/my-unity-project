using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_music.html")]
	public class Music : Soundtrack
	{
		public bool autoEndOtherMusicWhenPlayed = true;

		protected override bool IsMusic
		{
			get
			{
				return true;
			}
		}

		protected override List<MusicStorage> Storages
		{
			get
			{
				return KickStarter.settingsManager.musicStorages;
			}
		}

		protected new void Awake()
		{
			soundType = SoundType.Music;
			playWhilePaused = KickStarter.settingsManager.playMusicWhilePaused;
			base.Awake();
		}

		public override MainData SaveMainData(MainData mainData)
		{
			mainData.lastMusicQueueData = CreateLastSoundtrackString();
			mainData.musicQueueData = CreateTimesampleString();
			mainData.musicTimeSamples = 0;
			mainData.lastMusicTimeSamples = base.LastTimeSamples;
			if (GetCurrentTrackID() >= 0)
			{
				MusicStorage soundtrack = GetSoundtrack(GetCurrentTrackID());
				if (soundtrack != null && soundtrack.audioClip != null && base.audioSource.clip == soundtrack.audioClip && IsPlaying())
				{
					mainData.musicTimeSamples = base.audioSource.timeSamples;
				}
			}
			mainData.oldMusicTimeSamples = CreateOldTimesampleString();
			return mainData;
		}

		public override void LoadMainData(MainData mainData)
		{
			LoadMainData(mainData.musicTimeSamples, mainData.oldMusicTimeSamples, mainData.lastMusicTimeSamples, mainData.lastMusicQueueData, mainData.musicQueueData);
		}

		protected override bool EndsOthers()
		{
			return autoEndOtherMusicWhenPlayed;
		}
	}
}
