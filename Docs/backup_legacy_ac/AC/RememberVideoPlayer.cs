using UnityEngine;
using UnityEngine.Video;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Video Player")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_video_player.html")]
	public class RememberVideoPlayer : Remember
	{
		public bool saveClipAsset;

		private double loadTime;

		private bool playAfterLoad;

		public override string SaveData()
		{
			VideoPlayerData videoPlayerData = new VideoPlayerData();
			videoPlayerData.objectID = constantID;
			videoPlayerData.savePrevented = savePrevented;
			if ((bool)GetComponent<VideoPlayer>())
			{
				VideoPlayer component = GetComponent<VideoPlayer>();
				videoPlayerData.isPlaying = component.isPlaying;
				videoPlayerData.currentFrame = component.frame;
				videoPlayerData.currentTime = component.time;
				if (saveClipAsset && component.clip != null)
				{
					videoPlayerData.clipAssetID = AssetLoader.GetAssetInstanceID(component.clip);
				}
			}
			return Serializer.SaveScriptData<VideoPlayerData>(videoPlayerData);
		}

		public override void LoadData(string stringData)
		{
			VideoPlayerData videoPlayerData = Serializer.LoadScriptData<VideoPlayerData>(stringData);
			if (videoPlayerData == null)
			{
				return;
			}
			base.SavePrevented = videoPlayerData.savePrevented;
			if (savePrevented || !GetComponent<VideoPlayer>())
			{
				return;
			}
			VideoPlayer component = GetComponent<VideoPlayer>();
			if (saveClipAsset)
			{
				VideoClip videoClip = AssetLoader.RetrieveAsset(component.clip, videoPlayerData.clipAssetID);
				if (videoClip != null)
				{
					component.clip = videoClip;
				}
			}
			component.time = videoPlayerData.currentTime;
			if (videoPlayerData.isPlaying)
			{
				if (!component.isPrepared)
				{
					loadTime = videoPlayerData.currentTime;
					playAfterLoad = true;
					component.prepareCompleted += OnPrepareVideo;
					component.Prepare();
				}
				else
				{
					component.Play();
				}
			}
			else if (videoPlayerData.currentTime > 0.0)
			{
				if (!component.isPrepared)
				{
					loadTime = videoPlayerData.currentTime;
					playAfterLoad = false;
					component.prepareCompleted += OnPrepareVideo;
					component.Prepare();
				}
				else
				{
					component.Pause();
				}
			}
			else
			{
				component.Stop();
			}
		}

		private void OnPrepareVideo(VideoPlayer videoPlayer)
		{
			videoPlayer.time = loadTime;
			if (playAfterLoad)
			{
				videoPlayer.Play();
			}
			else
			{
				videoPlayer.Pause();
			}
		}
	}
}
