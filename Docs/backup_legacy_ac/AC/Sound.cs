using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Logic/Sound")]
	[RequireComponent(typeof(AudioSource))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_sound.html")]
	public class Sound : MonoBehaviour
	{
		[HideInInspector]
		public SoundType soundType;

		[HideInInspector]
		public bool playWhilePaused;

		[HideInInspector]
		public float relativeVolume = 1f;

		[HideInInspector]
		public bool surviveSceneChange;

		protected float maxVolume = 1f;

		protected float smoothVolume = 1f;

		protected float smoothUpdateSpeed = 20f;

		protected float fadeTime;

		protected float originalFadeTime;

		protected FadeType fadeType;

		protected Options options;

		protected float otherVolume = 1f;

		protected float originalRelativeVolume;

		protected float targetRelativeVolume;

		protected float relativeChangeTime;

		protected float originalRelativeChangeTime;

		public AudioSource audioSource { get; protected set; }

		protected void Awake()
		{
			Initialise();
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public virtual void _Update()
		{
			float num = Time.deltaTime;
			if (KickStarter.stateHandler.gameState == GameState.Paused)
			{
				if (!playWhilePaused)
				{
					return;
				}
				num = Time.fixedDeltaTime;
			}
			if (relativeChangeTime > 0f)
			{
				relativeChangeTime -= num;
				float num2 = (originalRelativeChangeTime - relativeChangeTime) / originalRelativeChangeTime;
				if (relativeChangeTime <= 0f)
				{
					relativeVolume = targetRelativeVolume;
				}
				else
				{
					relativeVolume = num2 * targetRelativeVolume + (1f - num2) * originalRelativeVolume;
				}
				SetMaxVolume();
			}
			if (fadeTime > 0f && audioSource.isPlaying)
			{
				smoothVolume = maxVolume;
				fadeTime -= num;
				float num3 = (originalFadeTime - fadeTime) / originalFadeTime;
				if (fadeType == FadeType.fadeIn)
				{
					if (num3 > 1f)
					{
						audioSource.volume = smoothVolume;
						fadeTime = 0f;
					}
					else
					{
						audioSource.volume = num3 * smoothVolume;
					}
				}
				else if (fadeType == FadeType.fadeOut)
				{
					if (num3 > 1f)
					{
						audioSource.volume = 0f;
						Stop();
					}
					else
					{
						audioSource.volume = (1f - num3) * smoothVolume;
					}
				}
				SetSmoothVolume();
			}
			else
			{
				SetSmoothVolume();
				if (audioSource != null)
				{
					audioSource.volume = smoothVolume;
				}
			}
		}

		public void AfterLoad()
		{
			ConstantID component = GetComponent<ConstantID>();
			if (!(component != null))
			{
				return;
			}
			Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			Sound[] array2 = array;
			foreach (Sound sound in array2)
			{
				if (!(sound != this))
				{
					continue;
				}
				ConstantID component2 = sound.GetComponent<ConstantID>();
				if (component2 != null && component2.constantID == component.constantID)
				{
					if (sound.IsPlaying())
					{
						Object.DestroyImmediate(base.gameObject);
					}
					else
					{
						Object.DestroyImmediate(sound.gameObject);
					}
					break;
				}
			}
		}

		public void AfterLoading()
		{
			if (audioSource == null)
			{
				audioSource = GetComponent<AudioSource>();
			}
			if (audioSource != null)
			{
				audioSource.ignoreListenerPause = playWhilePaused;
				if (audioSource.playOnAwake && audioSource.clip != null)
				{
					FadeIn(0.5f, audioSource.loop);
				}
				else
				{
					SetMaxVolume();
				}
				SnapSmoothVolume();
			}
			else
			{
				ACDebug.LogWarning("Sound object " + base.name + " has no AudioSource component.", this);
			}
		}

		public void Interact()
		{
			fadeTime = 0f;
			SetMaxVolume();
			Play(audioSource.loop);
		}

		public void FadeIn(float _fadeTime, bool loop, int _timeSamples = 0)
		{
			if (!(audioSource.clip == null))
			{
				audioSource.loop = loop;
				fadeTime = (originalFadeTime = _fadeTime);
				fadeType = FadeType.fadeIn;
				SetMaxVolume();
				audioSource.volume = 0f;
				audioSource.timeSamples = _timeSamples;
				audioSource.Play();
				KickStarter.eventManager.Call_OnPlaySound(this, audioSource, audioSource.clip, _fadeTime);
			}
		}

		public void FadeOut(float _fadeTime)
		{
			if (_fadeTime > 0f && audioSource.isPlaying)
			{
				fadeTime = (originalFadeTime = _fadeTime);
				fadeType = FadeType.fadeOut;
				SetMaxVolume();
				KickStarter.eventManager.Call_OnStopSound(this, audioSource, audioSource.clip, _fadeTime);
			}
			else
			{
				Stop();
			}
		}

		public bool IsFadingOut()
		{
			if (fadeTime > 0f && fadeType == FadeType.fadeOut)
			{
				return true;
			}
			return false;
		}

		public void Play()
		{
			if (!(audioSource == null))
			{
				fadeTime = 0f;
				SetMaxVolume();
				audioSource.Play();
				KickStarter.eventManager.Call_OnPlaySound(this, audioSource, audioSource.clip, 0f);
			}
		}

		public void Play(bool loop)
		{
			if (!(audioSource == null))
			{
				audioSource.loop = loop;
				audioSource.timeSamples = 0;
				Play();
			}
		}

		public void Play(AudioClip clip, bool loop, int _timeSamples = 0)
		{
			if (!(audioSource == null))
			{
				audioSource.clip = clip;
				audioSource.loop = loop;
				audioSource.timeSamples = _timeSamples;
				Play();
			}
		}

		public void PlayAtPoint(bool loop, int samplePoint)
		{
			if (!(audioSource == null))
			{
				audioSource.loop = loop;
				audioSource.timeSamples = samplePoint;
				Play();
			}
		}

		public void SetMaxVolume()
		{
			maxVolume = relativeVolume;
			if (KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				SetFinalVolume();
				return;
			}
			if (Options.optionsData != null)
			{
				if (soundType == SoundType.Music)
				{
					maxVolume *= Options.optionsData.musicVolume;
				}
				else if (soundType == SoundType.SFX)
				{
					maxVolume *= Options.optionsData.sfxVolume;
				}
				else if (soundType == SoundType.Speech)
				{
					maxVolume *= Options.optionsData.speechVolume;
				}
			}
			if (soundType == SoundType.Other)
			{
				maxVolume *= otherVolume;
			}
			SetFinalVolume();
		}

		public void SetVolume(float volume)
		{
			if (KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				volume = 1f;
			}
			maxVolume = relativeVolume * volume;
			otherVolume = volume;
			SetFinalVolume();
		}

		public void ChangeRelativeVolume(float newRelativeVolume, float changeTime = 0f)
		{
			if (changeTime <= 0f)
			{
				relativeVolume = newRelativeVolume;
				relativeChangeTime = 0f;
				SetMaxVolume();
			}
			else
			{
				originalRelativeVolume = relativeVolume;
				targetRelativeVolume = newRelativeVolume;
				relativeChangeTime = (originalRelativeChangeTime = changeTime);
			}
		}

		public void Stop()
		{
			AudioClip clip = audioSource.clip;
			fadeTime = 0f;
			audioSource.Stop();
			KickStarter.eventManager.Call_OnStopSound(this, audioSource, clip, 0f);
		}

		public bool IsFading()
		{
			return (fadeTime > 0f) ? true : false;
		}

		public bool IsPlaying()
		{
			if (audioSource == null)
			{
				Initialise();
			}
			if (audioSource != null)
			{
				if (KickStarter.stateHandler.IsPaused() && !playWhilePaused)
				{
					return audioSource.time > 0f;
				}
				return audioSource.isPlaying;
			}
			return false;
		}

		public bool IsPlaying(AudioClip clip)
		{
			if (audioSource != null && clip != null && audioSource.clip != null && audioSource.clip == clip && audioSource.isPlaying)
			{
				return true;
			}
			return false;
		}

		public void TryDestroy()
		{
			if (!(this is Music) && !(this is Ambience) && surviveSceneChange && !audioSource.isPlaying && base.gameObject.GetComponentInParent<Player>() == null && GetComponent<Player>() == null && GetComponentInChildren<Player>() == null)
			{
				ACDebug.Log(string.Concat("Deleting Sound object '", base.gameObject, "' as it is not currently playing any sound."), base.gameObject);
				Object.DestroyImmediate(base.gameObject);
			}
		}

		public void EndOld(SoundType _soundType, Sound ignoreSound)
		{
			if (soundType == _soundType && audioSource.isPlaying && this != ignoreSound && (fadeTime <= 0f || fadeType == FadeType.fadeIn))
			{
				FadeOut(0.1f);
			}
		}

		public SoundData GetSaveData(SoundData soundData)
		{
			soundData.isPlaying = IsPlaying();
			soundData.isLooping = audioSource.loop;
			soundData.samplePoint = audioSource.timeSamples;
			soundData.relativeVolume = relativeVolume;
			soundData.maxVolume = maxVolume;
			soundData.smoothVolume = smoothVolume;
			soundData.fadeTime = fadeTime;
			soundData.originalFadeTime = originalFadeTime;
			soundData.fadeType = (int)fadeType;
			soundData.otherVolume = otherVolume;
			soundData.originalRelativeVolume = originalRelativeVolume;
			soundData.targetRelativeVolume = targetRelativeVolume;
			soundData.relativeChangeTime = relativeChangeTime;
			soundData.originalRelativeChangeTime = originalRelativeChangeTime;
			if (audioSource.clip != null)
			{
				soundData.clipID = AssetLoader.GetAssetInstanceID(audioSource.clip);
			}
			return soundData;
		}

		public void LoadData(SoundData soundData)
		{
			if (soundData.isPlaying)
			{
				audioSource.clip = AssetLoader.RetrieveAsset(audioSource.clip, soundData.clipID);
				PlayAtPoint(soundData.isLooping, soundData.samplePoint);
			}
			else
			{
				Stop();
			}
			relativeVolume = soundData.relativeVolume;
			maxVolume = soundData.maxVolume;
			smoothVolume = soundData.smoothVolume;
			fadeTime = soundData.fadeTime;
			originalFadeTime = soundData.originalFadeTime;
			fadeType = (FadeType)soundData.fadeType;
			otherVolume = soundData.otherVolume;
			originalRelativeVolume = soundData.originalRelativeVolume;
			targetRelativeVolume = soundData.targetRelativeVolume;
			relativeChangeTime = soundData.relativeChangeTime;
			originalRelativeChangeTime = soundData.originalRelativeChangeTime;
		}

		protected void Initialise()
		{
			if (surviveSceneChange)
			{
				if (base.transform.root != null && base.transform.root != base.gameObject.transform)
				{
					base.transform.SetParent(null);
				}
				Object.DontDestroyOnLoad(this);
			}
			audioSource = GetComponent<AudioSource>();
			if (audioSource != null)
			{
				if (audioSource.playOnAwake)
				{
					audioSource.playOnAwake = false;
				}
				audioSource.ignoreListenerPause = playWhilePaused;
				AdvGame.AssignMixerGroup(audioSource, soundType);
			}
		}

		protected void SetSmoothVolume()
		{
			if (!Mathf.Approximately(smoothVolume, maxVolume))
			{
				if (smoothUpdateSpeed > 0f)
				{
					smoothVolume = Mathf.Lerp(smoothVolume, maxVolume, (KickStarter.stateHandler.gameState != GameState.Paused) ? (Time.deltaTime * smoothUpdateSpeed) : Time.fixedDeltaTime);
				}
				else
				{
					SnapSmoothVolume();
				}
			}
		}

		protected void SnapSmoothVolume()
		{
			smoothVolume = maxVolume;
		}

		protected void SetFinalVolume()
		{
			if (KickStarter.dialog.AudioIsPlaying())
			{
				if (soundType == SoundType.SFX)
				{
					maxVolume *= 1f - KickStarter.speechManager.sfxDucking;
				}
				else if (soundType == SoundType.Music)
				{
					maxVolume *= 1f - KickStarter.speechManager.musicDucking;
				}
			}
		}

		protected void TurnOn()
		{
			audioSource.timeSamples = 0;
			Play();
		}

		protected void TurnOff()
		{
			FadeOut(0.2f);
		}

		protected void Kill()
		{
			Stop();
		}
	}
}
