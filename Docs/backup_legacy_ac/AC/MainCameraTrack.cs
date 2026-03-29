using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[Serializable]
	[TrackClipType(typeof(MainCameraShot))]
	[TrackColor(0.73f, 0.1f, 0.1f)]
	public class MainCameraTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			ScriptPlayable<MainCameraMixer> scriptPlayable = ScriptPlayable<MainCameraMixer>.Create(graph);
			scriptPlayable.SetInputCount(inputCount);
			return scriptPlayable;
		}
	}
}
