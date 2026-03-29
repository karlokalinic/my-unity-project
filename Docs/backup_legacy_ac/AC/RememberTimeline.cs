using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[RequireComponent(typeof(PlayableDirector))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Timeline")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_timeline.html")]
	public class RememberTimeline : Remember
	{
		public bool saveBindings;

		public bool saveTimelineAsset;

		public override string SaveData()
		{
			TimelineData timelineData = new TimelineData();
			timelineData.objectID = constantID;
			timelineData.savePrevented = savePrevented;
			PlayableDirector component = GetComponent<PlayableDirector>();
			timelineData.isPlaying = component.state == PlayState.Playing;
			timelineData.currentTime = component.time;
			timelineData.trackObjectData = string.Empty;
			timelineData.timelineAssetID = string.Empty;
			if (component.playableAsset != null)
			{
				TimelineAsset timelineAsset = (TimelineAsset)component.playableAsset;
				if (timelineAsset != null)
				{
					if (saveTimelineAsset)
					{
						timelineData.timelineAssetID = AssetLoader.GetAssetInstanceID(timelineAsset);
					}
					if (saveBindings)
					{
						int[] array = new int[timelineAsset.outputTrackCount];
						for (int i = 0; i < array.Length; i++)
						{
							TrackAsset outputTrack = timelineAsset.GetOutputTrack(i);
							GameObject gameObject = component.GetGenericBinding(outputTrack) as GameObject;
							array[i] = 0;
							if (gameObject != null)
							{
								ConstantID component2 = gameObject.GetComponent<ConstantID>();
								if (component2 != null)
								{
									array[i] = component2.constantID;
								}
							}
						}
						for (int j = 0; j < array.Length; j++)
						{
							timelineData.trackObjectData += array[j];
							if (j < array.Length - 1)
							{
								timelineData.trackObjectData += ",";
							}
						}
					}
				}
			}
			return Serializer.SaveScriptData<TimelineData>(timelineData);
		}

		public override void LoadData(string stringData, bool restoringSaveFile = false)
		{
			TimelineData timelineData = Serializer.LoadScriptData<TimelineData>(stringData);
			if (timelineData == null)
			{
				return;
			}
			base.SavePrevented = timelineData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			PlayableDirector component = GetComponent<PlayableDirector>();
			if (component != null && component.playableAsset != null)
			{
				TimelineAsset timelineAsset = (TimelineAsset)component.playableAsset;
				if (timelineAsset != null)
				{
					if (saveTimelineAsset)
					{
						TimelineAsset timelineAsset2 = AssetLoader.RetrieveAsset(timelineAsset, timelineData.timelineAssetID);
						if (timelineAsset2 != null)
						{
							component.playableAsset = timelineAsset2;
							timelineAsset = timelineAsset2;
						}
					}
					if (saveBindings && !string.IsNullOrEmpty(timelineData.trackObjectData))
					{
						string[] array = timelineData.trackObjectData.Split(","[0]);
						for (int i = 0; i < array.Length; i++)
						{
							int result = 0;
							if (!int.TryParse(array[i], out result) || result == 0)
							{
								continue;
							}
							TrackAsset outputTrack = timelineAsset.GetOutputTrack(i);
							if (outputTrack != null)
							{
								ConstantID constantID = Serializer.returnComponent<ConstantID>(result, base.gameObject);
								if (constantID != null)
								{
									component.SetGenericBinding(outputTrack, constantID.gameObject);
								}
							}
						}
					}
				}
			}
			component.time = timelineData.currentTime;
			if (timelineData.isPlaying)
			{
				component.Play();
			}
			else
			{
				component.Stop();
			}
		}
	}
}
