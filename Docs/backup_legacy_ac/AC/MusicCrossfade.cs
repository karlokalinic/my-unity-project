using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(AudioSource))]
	public class MusicCrossfade : MonoBehaviour
	{
		protected AudioSource _audioSource;

		protected bool isFadingOut;

		protected float fadeTime;

		protected float originalFadeTime;

		protected float originalVolume;

		protected bool isPlaying;

		protected void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
			_audioSource.ignoreListenerPause = KickStarter.settingsManager.playMusicWhilePaused;
		}

		public void _Update()
		{
			if (isFadingOut)
			{
				float num = fadeTime / originalFadeTime;
				_audioSource.volume = originalVolume * num;
				if (Mathf.Approximately(Time.time, 0f))
				{
					fadeTime -= Time.fixedDeltaTime;
				}
				else
				{
					fadeTime -= Time.deltaTime;
				}
				if (fadeTime <= 0f)
				{
					Stop();
				}
			}
		}

		public void Stop()
		{
			isFadingOut = false;
			_audioSource.Stop();
			isPlaying = false;
		}

		public bool IsPlaying()
		{
			return isPlaying;
		}

		public void FadeOut(AudioSource audioSourceToCopy, float _fadeTime)
		{
			Stop();
			if (!(audioSourceToCopy == null) && !(audioSourceToCopy.clip == null) && !(_fadeTime <= 0f))
			{
				_audioSource.clip = audioSourceToCopy.clip;
				_audioSource.outputAudioMixerGroup = audioSourceToCopy.outputAudioMixerGroup;
				_audioSource.volume = audioSourceToCopy.volume;
				_audioSource.timeSamples = audioSourceToCopy.timeSamples;
				_audioSource.loop = false;
				_audioSource.Play();
				isPlaying = true;
				originalFadeTime = _fadeTime;
				originalVolume = audioSourceToCopy.volume;
				fadeTime = _fadeTime;
				isFadingOut = true;
			}
		}
	}
}
