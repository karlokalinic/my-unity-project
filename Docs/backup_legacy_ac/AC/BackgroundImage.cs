using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_background_image.html")]
	public class BackgroundImage : MonoBehaviour
	{
		public enum BackgroundImageSource
		{
			Texture = 0,
			VideoClip = 1
		}

		public BackgroundImageSource backgroundImageSource;

		public VideoClip backgroundVideo;

		protected VideoPlayer videoPlayer;

		public Texture backgroundTexture;

		public bool loopMovie = true;

		public bool restartMovieWhenTurnOn;

		protected float shakeDuration;

		protected float startTime;

		protected float startShakeIntensity;

		protected float shakeIntensity;

		protected Rect originalPixelInset;

		protected void Awake()
		{
			PrepareVideo();
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

		public void SetImage(Texture2D _texture)
		{
			SetBackgroundTexture(_texture);
		}

		public void TurnOn()
		{
			if (LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer) == -1)
			{
				ACDebug.LogWarning("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.backgroundImageLayer);
			}
			TurnOnUI();
			if (backgroundImageSource == BackgroundImageSource.VideoClip && videoPlayer != null)
			{
				if (restartMovieWhenTurnOn)
				{
					videoPlayer.Stop();
				}
				videoPlayer.isLooping = loopMovie;
			}
		}

		public void TurnOff()
		{
			base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			TurnOffUI();
		}

		public void Shake(float _shakeIntensity, float _duration)
		{
			shakeDuration = _duration;
			startTime = Time.time;
			shakeIntensity = _shakeIntensity;
			startShakeIntensity = shakeIntensity;
			StopCoroutine(UpdateShake());
			StartCoroutine(UpdateShake());
		}

		public void CancelVideoPlayback()
		{
			if (videoPlayer != null)
			{
				videoPlayer.Stop();
			}
			StopCoroutine("PlayVideoCoroutine");
		}

		protected void TurnOnUI()
		{
			SetBackgroundCameraFarClipPlane(0.02f);
			BackgroundImageUI.Instance.SetTexture(backgroundTexture);
			if (Application.isPlaying && backgroundImageSource == BackgroundImageSource.VideoClip)
			{
				StartCoroutine(PlayVideoCoroutine());
			}
		}

		protected void TurnOffUI()
		{
			if (backgroundImageSource == BackgroundImageSource.VideoClip && Application.isPlaying)
			{
				videoPlayer.Stop();
				if (videoPlayer.isPrepared)
				{
					if (videoPlayer.texture != null)
					{
						BackgroundImageUI.Instance.ClearTexture(videoPlayer.texture);
					}
					return;
				}
			}
			Texture texture = backgroundTexture;
			if (texture != null)
			{
				BackgroundImageUI.Instance.ClearTexture(texture);
			}
		}

		protected void SetBackgroundCameraFarClipPlane(float value)
		{
			BackgroundCamera backgroundCamera = Object.FindObjectOfType<BackgroundCamera>();
			if ((bool)backgroundCamera)
			{
				backgroundCamera.GetComponent<Camera>().farClipPlane = value;
			}
			else
			{
				ACDebug.LogWarning("Cannot find BackgroundCamera");
			}
		}

		protected IEnumerator UpdateShake()
		{
			while (shakeIntensity > 0f)
			{
				float _size = Random.Range(0f, shakeIntensity) * 0.2f;
				BackgroundImageUI.Instance.SetShakeIntensity(_size);
				shakeIntensity = Mathf.Lerp(startShakeIntensity, 0f, AdvGame.Interpolate(startTime, shakeDuration, MoveMethod.Linear));
				yield return new WaitForEndOfFrame();
			}
			shakeIntensity = 0f;
			BackgroundImageUI.Instance.SetShakeIntensity(0f);
		}

		protected void SetBackgroundTexture(Texture _texture)
		{
			backgroundTexture = _texture;
		}

		protected IEnumerator PlayVideoCoroutine()
		{
			foreach (BackgroundImage backgroundImage in KickStarter.stateHandler.BackgroundImages)
			{
				if (backgroundImage != null)
				{
					backgroundImage.CancelVideoPlayback();
				}
			}
			yield return new WaitForEndOfFrame();
			videoPlayer.Prepare();
			while (!videoPlayer.isPrepared)
			{
				yield return new WaitForEndOfFrame();
			}
			videoPlayer.Play();
			yield return new WaitForEndOfFrame();
			BackgroundImageUI.Instance.SetTexture(videoPlayer.texture);
		}

		protected void PrepareVideo()
		{
			if (backgroundImageSource == BackgroundImageSource.VideoClip)
			{
				videoPlayer = GetComponent<VideoPlayer>();
				if (videoPlayer == null)
				{
					videoPlayer = base.gameObject.AddComponent<VideoPlayer>();
					videoPlayer.isLooping = true;
				}
				videoPlayer.playOnAwake = false;
				videoPlayer.renderMode = VideoRenderMode.APIOnly;
				videoPlayer.clip = backgroundVideo;
			}
		}
	}
}
