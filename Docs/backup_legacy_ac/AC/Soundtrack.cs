using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_sountrack.html")]
	public abstract class Soundtrack : Sound
	{
		protected struct QueuedSoundtrack
		{
			public int trackID;

			public bool trackLoop;

			public float fadeTime;

			public bool isCrossfade;

			public bool doResume;

			public int newTimeSamples;

			public float loopingOverlapTime;

			public QueuedSoundtrack(int _trackID, bool _trackLoop, float _fadeTime = 0f, bool _isCrossfade = false, bool _doResume = false, int _newTimeSamples = 0, float _loopingOverlapTime = 0f)
			{
				trackID = _trackID;
				trackLoop = _trackLoop;
				fadeTime = _fadeTime;
				doResume = _doResume;
				newTimeSamples = _newTimeSamples;
				loopingOverlapTime = _loopingOverlapTime;
				isCrossfade = fadeTime > 0f && _isCrossfade;
			}

			public QueuedSoundtrack(QueuedSoundtrack _queuedSoundtrack)
			{
				trackID = _queuedSoundtrack.trackID;
				trackLoop = _queuedSoundtrack.trackLoop;
				fadeTime = _queuedSoundtrack.fadeTime;
				isCrossfade = _queuedSoundtrack.isCrossfade;
				doResume = false;
				newTimeSamples = 0;
				loopingOverlapTime = _queuedSoundtrack.loopingOverlapTime;
			}
		}

		protected struct SoundtrackSample
		{
			public int trackID;

			public int timeSample;

			public SoundtrackSample(int _trackID, int _timeSample)
			{
				trackID = _trackID;
				timeSample = _timeSample;
			}
		}

		public float loadFadeTime;

		protected List<QueuedSoundtrack> queuedSoundtrack = new List<QueuedSoundtrack>();

		protected MusicCrossfade crossfade;

		protected List<QueuedSoundtrack> lastQueuedSoundtrack = new List<QueuedSoundtrack>();

		protected List<SoundtrackSample> oldSoundtrackSamples = new List<SoundtrackSample>();

		protected int lastTimeSamples;

		protected float delayTime;

		protected int delayAudioID = -1;

		protected float delayFadeTime;

		protected bool delayLoop;

		protected bool delayResumeIfPlayedBefore;

		protected int delayNewTrackTimeSamples;

		protected bool wasPlayingLastFrame;

		protected virtual List<MusicStorage> Storages
		{
			get
			{
				return null;
			}
		}

		protected int LastTimeSamples
		{
			get
			{
				return lastTimeSamples;
			}
		}

		protected virtual bool IsMusic
		{
			get
			{
				return false;
			}
		}

		protected new void Awake()
		{
			crossfade = GetComponentInChildren<MusicCrossfade>();
			surviveSceneChange = true;
			Initialise();
			queuedSoundtrack.Clear();
			lastQueuedSoundtrack.Clear();
			if (crossfade == null)
			{
				ACDebug.LogWarning("The " + base.gameObject.name + " requires a 'MusicCrossfade' component to be attached as a child component.\r\nOne has been added automatically, but you should update the source prefab.", base.gameObject);
				GameObject gameObject = new GameObject("Crossfader");
				gameObject.AddComponent<AudioSource>();
				crossfade = gameObject.AddComponent<MusicCrossfade>();
				gameObject.transform.position = base.transform.position;
				gameObject.transform.parent = base.transform;
			}
		}

		public override void _Update()
		{
			float num = Time.deltaTime;
			if (KickStarter.stateHandler.gameState == GameState.Paused)
			{
				if (soundType != SoundType.Music || !KickStarter.settingsManager.playMusicWhilePaused)
				{
					return;
				}
				num = Time.fixedDeltaTime;
			}
			if ((bool)crossfade)
			{
				crossfade._Update();
			}
			if (delayAudioID >= 0 && delayTime > 0f)
			{
				delayTime -= num;
				if (delayTime <= 0f)
				{
					AfterDelay();
				}
				base._Update();
			}
			if (this.queuedSoundtrack.Count > 0 && delayAudioID < 0)
			{
				if (!IsPlaying())
				{
					ClearSoundtrackSample(this.queuedSoundtrack[0].trackID);
					this.queuedSoundtrack.RemoveAt(0);
					if (this.queuedSoundtrack.Count > 0)
					{
						MusicStorage soundtrack = GetSoundtrack(this.queuedSoundtrack[0].trackID);
						if (soundtrack != null && soundtrack.audioClip != null)
						{
							int timeSamples = ((!this.queuedSoundtrack[0].doResume) ? this.queuedSoundtrack[0].newTimeSamples : GetSoundtrackSample(this.queuedSoundtrack[0].trackID));
							SetRelativeVolume(soundtrack.relativeVolume);
							Play(soundtrack.audioClip, this.queuedSoundtrack[0].trackLoop, timeSamples);
						}
					}
				}
				else if (this.queuedSoundtrack.Count > 1 && delayAudioID < 0)
				{
					QueuedSoundtrack queuedSoundtrack = this.queuedSoundtrack[1];
					if (queuedSoundtrack.fadeTime > 0f)
					{
						int timeSamples2 = ((!queuedSoundtrack.doResume) ? queuedSoundtrack.newTimeSamples : GetSoundtrackSample(queuedSoundtrack.trackID));
						float num2 = (base.audioSource.clip.length - queuedSoundtrack.fadeTime) / base.audioSource.clip.length;
						int num3 = (int)(num2 * (float)base.audioSource.clip.samples);
						if (base.audioSource.timeSamples > num3)
						{
							MusicStorage soundtrack2 = GetSoundtrack(queuedSoundtrack.trackID);
							ClearSoundtrackSample(this.queuedSoundtrack[0].trackID);
							this.queuedSoundtrack.RemoveAt(0);
							if (queuedSoundtrack.isCrossfade)
							{
								if ((bool)crossfade)
								{
									crossfade.FadeOut(base.audioSource, queuedSoundtrack.fadeTime);
								}
								base.audioSource.clip = soundtrack2.audioClip;
								SetRelativeVolume(soundtrack2.relativeVolume);
								HandleFadeIn(queuedSoundtrack.fadeTime, queuedSoundtrack.trackLoop, timeSamples2);
							}
							else
							{
								FadeOutThenIn(soundtrack2, queuedSoundtrack.fadeTime, queuedSoundtrack.trackLoop, queuedSoundtrack.doResume, queuedSoundtrack.newTimeSamples);
							}
						}
					}
				}
				else if (this.queuedSoundtrack.Count == 1 && delayAudioID < 0 && this.queuedSoundtrack[0].trackLoop && this.queuedSoundtrack[0].loopingOverlapTime > 0f)
				{
					float num4 = (base.audioSource.clip.length - this.queuedSoundtrack[0].loopingOverlapTime) / base.audioSource.clip.length;
					int num5 = (int)(num4 * (float)base.audioSource.clip.samples);
					if (base.audioSource.timeSamples > num5)
					{
						crossfade.FadeOut(base.audioSource, this.queuedSoundtrack[0].loopingOverlapTime);
						HandleFadeIn(this.queuedSoundtrack[0].loopingOverlapTime, true, 0);
					}
				}
			}
			base._Update();
			if (!IsPlaying() && wasPlayingLastFrame)
			{
				KickStarter.eventManager.Call_OnStopSoundtrack(IsMusic, 0f);
			}
			wasPlayingLastFrame = IsPlaying();
		}

		public float Play(int trackID, bool loop, bool isQueued, float fadeTime, bool resumeIfPlayedBefore = false, int newTrackTimeSamples = 0, float loopingOverlapTime = 0f)
		{
			return HandlePlay(trackID, loop, isQueued, fadeTime, false, resumeIfPlayedBefore, newTrackTimeSamples, loopingOverlapTime);
		}

		public float Crossfade(int trackID, bool loop, bool isQueued, float fadeTime, bool resumeIfPlayedBefore = false, int newTrackTimeSamples = 0, float loopingOverlapTime = 0f)
		{
			return HandlePlay(trackID, loop, isQueued, fadeTime, true, resumeIfPlayedBefore, newTrackTimeSamples, loopingOverlapTime);
		}

		public float ResumeLastQueue(float fadeTime, bool playFromStart)
		{
			if (lastQueuedSoundtrack.Count == 0)
			{
				ACDebug.LogWarning("Can't resume track - nothing in the queue!", this);
				return 0f;
			}
			if ((queuedSoundtrack.Count > 0) ? true : false)
			{
				ACDebug.LogWarning("Can't resume last stopped track, as a track is already playing.", this);
				return 0f;
			}
			queuedSoundtrack.Clear();
			foreach (QueuedSoundtrack item in lastQueuedSoundtrack)
			{
				queuedSoundtrack.Add(new QueuedSoundtrack(item));
			}
			Resume((!playFromStart) ? lastTimeSamples : 0, fadeTime);
			return fadeTime;
		}

		public float StopAll(float fadeTime, bool storeCurrentIndex = true)
		{
			if (queuedSoundtrack.Count == 0 && base.audioSource != null && !IsPlaying() && (crossfade == null || !crossfade.IsPlaying()))
			{
				return 0f;
			}
			return ForceStopAll(fadeTime, storeCurrentIndex);
		}

		public virtual MainData SaveMainData(MainData mainData)
		{
			return mainData;
		}

		public virtual void LoadMainData(MainData mainData)
		{
		}

		public int GetCurrentTrackID()
		{
			if (queuedSoundtrack.Count > 0)
			{
				return queuedSoundtrack[0].trackID;
			}
			return -1;
		}

		public void SyncTrackWithCurrent(int trackID, int sampleOffset = 0)
		{
			int timeSamples = base.audioSource.timeSamples;
			SetSoundtrackSample(trackID, Mathf.Max(0, timeSamples + sampleOffset));
		}

		protected void EndOthers()
		{
			if (EndsOthers())
			{
				Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
				Sound[] array2 = array;
				foreach (Sound sound in array2)
				{
					sound.EndOld(soundType, this);
				}
			}
		}

		protected virtual bool EndsOthers()
		{
			return false;
		}

		protected float HandlePlay(int trackID, bool loop, bool isQueued, float fadeTime, bool isCrossfade, bool resumeIfPlayedBefore, int newTrackTimeSamples = 0, float loopingOverlapTime = 0f)
		{
			if ((bool)crossfade)
			{
				crossfade.Stop();
			}
			MusicStorage soundtrack = GetSoundtrack(trackID);
			if (soundtrack == null || soundtrack.audioClip == null)
			{
				ACDebug.LogWarning("Cannot play " + base.name + " - no AudioClip assigned to track " + trackID + "!");
				return 0f;
			}
			if (isQueued && queuedSoundtrack.Count > 0)
			{
				queuedSoundtrack.Add(new QueuedSoundtrack(trackID, loop, fadeTime, isCrossfade, resumeIfPlayedBefore, newTrackTimeSamples, loopingOverlapTime));
				return 0f;
			}
			if (queuedSoundtrack.Count > 0 && queuedSoundtrack[0].trackID == trackID)
			{
				return 0f;
			}
			EndOthers();
			bool flag = ((queuedSoundtrack.Count > 0) ? true : false);
			if (flag)
			{
				StoreSoundtrackSampleByIndex(0);
			}
			if (resumeIfPlayedBefore)
			{
				newTrackTimeSamples = GetSoundtrackSample(trackID);
			}
			queuedSoundtrack.Clear();
			if (loop && loopingOverlapTime > 0f)
			{
				queuedSoundtrack.Add(new QueuedSoundtrack(trackID, loop, fadeTime, false, false, 0, loopingOverlapTime));
			}
			else
			{
				queuedSoundtrack.Add(new QueuedSoundtrack(trackID, loop));
			}
			KickStarter.eventManager.Call_OnPlaySoundtrack(trackID, IsMusic, loop, fadeTime, newTrackTimeSamples);
			if (flag)
			{
				if (fadeTime > 0f)
				{
					if (isCrossfade)
					{
						if ((bool)crossfade)
						{
							crossfade.FadeOut(base.audioSource, fadeTime);
						}
						SetRelativeVolume(soundtrack.relativeVolume);
						base.audioSource.clip = soundtrack.audioClip;
						HandleFadeIn(fadeTime, loop, newTrackTimeSamples);
						return fadeTime;
					}
					FadeOutThenIn(soundtrack, fadeTime, loop, resumeIfPlayedBefore, newTrackTimeSamples);
					return fadeTime * 2f;
				}
				Stop();
				SetRelativeVolume(soundtrack.relativeVolume);
				Play(soundtrack.audioClip, loop, newTrackTimeSamples);
				return 0f;
			}
			SetRelativeVolume(soundtrack.relativeVolume);
			if (fadeTime <= 0f && KickStarter.stateHandler.gameState != GameState.Paused)
			{
				fadeTime = 0.001f;
			}
			if (fadeTime > 0f)
			{
				base.audioSource.clip = soundtrack.audioClip;
				HandleFadeIn(fadeTime, loop, newTrackTimeSamples);
				return fadeTime;
			}
			Play(soundtrack.audioClip, loop, newTrackTimeSamples);
			return 0f;
		}

		protected float ForceStopAll(float fadeTime, bool storeCurrentIndex = true)
		{
			if (fadeTime <= 0f && (bool)crossfade)
			{
				crossfade.Stop();
			}
			if (storeCurrentIndex)
			{
				StoreSoundtrackSampleByIndex(0);
			}
			delayAudioID = -1;
			ClearSoundtrackQueue();
			wasPlayingLastFrame = false;
			KickStarter.eventManager.Call_OnStopSoundtrack(IsMusic, fadeTime);
			if (fadeTime > 0f && IsPlaying())
			{
				FadeOut(fadeTime);
				return fadeTime;
			}
			Stop();
			return 0f;
		}

		protected void ClearSoundtrackQueue()
		{
			lastTimeSamples = 0;
			if (queuedSoundtrack != null && queuedSoundtrack.Count > 0)
			{
				MusicStorage soundtrack = GetSoundtrack(queuedSoundtrack[0].trackID);
				if (soundtrack != null && soundtrack.audioClip != null && base.audioSource.clip == soundtrack.audioClip && IsPlaying())
				{
					lastTimeSamples = base.audioSource.timeSamples;
				}
			}
			lastQueuedSoundtrack.Clear();
			foreach (QueuedSoundtrack item in queuedSoundtrack)
			{
				lastQueuedSoundtrack.Add(new QueuedSoundtrack(item));
			}
			queuedSoundtrack.Clear();
		}

		protected void FadeOutThenIn(MusicStorage musicStorage, float fadeTime, bool loop, bool resumeIfPlayedBefore, int newTrackTimeSamples)
		{
			FadeOut(fadeTime);
			delayTime = fadeTime;
			delayAudioID = musicStorage.ID;
			delayFadeTime = fadeTime;
			delayLoop = loop;
			delayResumeIfPlayedBefore = resumeIfPlayedBefore;
			delayNewTrackTimeSamples = newTrackTimeSamples;
		}

		protected void AfterDelay()
		{
			if (delayAudioID >= 0)
			{
				delayTime = 0f;
				MusicStorage soundtrack = GetSoundtrack(delayAudioID);
				if (soundtrack != null)
				{
					int timeSamples = ((!delayResumeIfPlayedBefore) ? delayNewTrackTimeSamples : GetSoundtrackSample(delayAudioID));
					base.audioSource.clip = soundtrack.audioClip;
					SetRelativeVolume(soundtrack.relativeVolume);
					FadeIn(delayFadeTime, delayLoop, timeSamples);
				}
			}
			delayAudioID = -1;
		}

		protected void Resume(int _timeSamples, float fadeTime = 0f)
		{
			if (queuedSoundtrack.Count <= 0)
			{
				return;
			}
			MusicStorage soundtrack = GetSoundtrack(queuedSoundtrack[0].trackID);
			if (soundtrack != null && soundtrack.audioClip != null)
			{
				base.audioSource.clip = soundtrack.audioClip;
				SetRelativeVolume(soundtrack.relativeVolume);
				PlayAtPoint(queuedSoundtrack[0].trackLoop, _timeSamples);
				if (fadeTime > 0f)
				{
					HandleFadeIn(fadeTime, queuedSoundtrack[0].trackLoop, _timeSamples);
				}
			}
		}

		protected void HandleFadeIn(float _fadeTime, bool loop, int _timeSamples)
		{
			FadeIn(_fadeTime, loop, _timeSamples);
		}

		protected string CreateTimesampleString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < queuedSoundtrack.Count; i++)
			{
				stringBuilder.Append(queuedSoundtrack[i].trackID.ToString());
				stringBuilder.Append(":");
				stringBuilder.Append((!queuedSoundtrack[i].trackLoop) ? "0" : "1");
				stringBuilder.Append(":");
				stringBuilder.Append(queuedSoundtrack[i].fadeTime);
				stringBuilder.Append(":");
				stringBuilder.Append((!queuedSoundtrack[i].isCrossfade) ? "0" : "1");
				stringBuilder.Append(":");
				stringBuilder.Append(queuedSoundtrack[i].loopingOverlapTime);
				if (i < queuedSoundtrack.Count - 1)
				{
					stringBuilder.Append("|");
				}
			}
			return stringBuilder.ToString();
		}

		protected string CreateLastSoundtrackString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < lastQueuedSoundtrack.Count; i++)
			{
				stringBuilder.Append(lastQueuedSoundtrack[i].trackID.ToString());
				stringBuilder.Append(":");
				stringBuilder.Append((!lastQueuedSoundtrack[i].trackLoop) ? "0" : "1");
				stringBuilder.Append(":");
				stringBuilder.Append(lastQueuedSoundtrack[i].fadeTime);
				stringBuilder.Append(":");
				stringBuilder.Append((!lastQueuedSoundtrack[i].isCrossfade) ? "0" : "1");
				stringBuilder.Append(":");
				stringBuilder.Append(lastQueuedSoundtrack[i].loopingOverlapTime);
				if (i < lastQueuedSoundtrack.Count - 1)
				{
					stringBuilder.Append("|");
				}
			}
			return stringBuilder.ToString();
		}

		protected string CreateOldTimesampleString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < oldSoundtrackSamples.Count; i++)
			{
				stringBuilder.Append(oldSoundtrackSamples[i].trackID.ToString());
				stringBuilder.Append(":");
				stringBuilder.Append(oldSoundtrackSamples[i].timeSample.ToString());
				if (i < oldSoundtrackSamples.Count - 1)
				{
					stringBuilder.Append("|");
				}
			}
			return stringBuilder.ToString();
		}

		protected void LoadMainData(int _timeSamples, string _oldTimeSamples, int _lastTimeSamples, string _lastQueueData, string _queueData)
		{
			ForceStopAll(0f, false);
			if (!string.IsNullOrEmpty(_oldTimeSamples))
			{
				oldSoundtrackSamples.Clear();
				string[] array = _oldTimeSamples.Split("|"[0]);
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(":"[0]);
					int result = 0;
					int.TryParse(array3[0], out result);
					int result2 = 0;
					int.TryParse(array3[1], out result2);
					oldSoundtrackSamples.Add(new SoundtrackSample(result, result2));
				}
			}
			lastTimeSamples = _lastTimeSamples;
			if (!string.IsNullOrEmpty(_lastQueueData))
			{
				lastQueuedSoundtrack.Clear();
				string[] array4 = _lastQueueData.Split("|"[0]);
				string[] array5 = array4;
				foreach (string text2 in array5)
				{
					string[] array6 = text2.Split(":"[0]);
					int result3 = 0;
					int.TryParse(array6[0], out result3);
					int result4 = 0;
					int.TryParse(array6[1], out result4);
					bool trackLoop = result4 == 1;
					float result5 = 0f;
					float.TryParse(array6[2], out result5);
					int result6 = 0;
					int.TryParse(array6[3], out result6);
					bool isCrossfade = result6 == 1;
					float result7 = 0f;
					if (array6.Length >= 5)
					{
						float.TryParse(array6[4], out result7);
					}
					lastQueuedSoundtrack.Add(new QueuedSoundtrack(result3, trackLoop, result5, isCrossfade, false, 0, result7));
				}
			}
			if (string.IsNullOrEmpty(_queueData))
			{
				return;
			}
			string[] array7 = _queueData.Split("|"[0]);
			string[] array8 = array7;
			foreach (string text3 in array8)
			{
				string[] array9 = text3.Split(":"[0]);
				int result8 = 0;
				int.TryParse(array9[0], out result8);
				int result9 = 0;
				int.TryParse(array9[1], out result9);
				bool trackLoop2 = result9 == 1;
				float result10 = 0f;
				float.TryParse(array9[2], out result10);
				int result11 = 0;
				int.TryParse(array9[3], out result11);
				bool isCrossfade2 = result11 == 1;
				float result12 = 0f;
				if (array9.Length >= 5)
				{
					float.TryParse(array9[4], out result12);
				}
				queuedSoundtrack.Add(new QueuedSoundtrack(result8, trackLoop2, result10, isCrossfade2, false, 0, result12));
			}
			Resume(_timeSamples, loadFadeTime);
		}

		protected MusicStorage GetSoundtrack(int ID)
		{
			foreach (MusicStorage storage in Storages)
			{
				if (storage.ID == ID)
				{
					return storage;
				}
			}
			return null;
		}

		protected void SetRelativeVolume(float _relativeVolume)
		{
			relativeVolume = _relativeVolume;
			SetMaxVolume();
		}

		protected void StoreSoundtrackSampleByIndex(int index)
		{
			if (queuedSoundtrack != null && queuedSoundtrack.Count > index)
			{
				int trackID = queuedSoundtrack[index].trackID;
				MusicStorage soundtrack = GetSoundtrack(trackID);
				if (soundtrack != null && soundtrack.audioClip != null && base.audioSource.clip == soundtrack.audioClip && IsPlaying())
				{
					SetSoundtrackSample(trackID, base.audioSource.timeSamples);
				}
			}
		}

		protected int GetSoundtrackSample(int trackID)
		{
			foreach (SoundtrackSample oldSoundtrackSample in oldSoundtrackSamples)
			{
				if (oldSoundtrackSample.trackID == trackID)
				{
					return oldSoundtrackSample.timeSample;
				}
			}
			return 0;
		}

		protected void ClearSoundtrackSample(int trackID)
		{
			foreach (SoundtrackSample oldSoundtrackSample in oldSoundtrackSamples)
			{
				if (oldSoundtrackSample.trackID == trackID)
				{
					oldSoundtrackSamples.Remove(oldSoundtrackSample);
					break;
				}
			}
		}

		protected void SetSoundtrackSample(int trackID, int timeSample)
		{
			ClearSoundtrackSample(trackID);
			SoundtrackSample item = new SoundtrackSample(trackID, timeSample);
			oldSoundtrackSamples.Add(item);
		}
	}
}
