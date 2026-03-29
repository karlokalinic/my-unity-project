using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[Serializable]
	[TrackClipType(typeof(CameraFadeShot))]
	[TrackColor(0.1f, 0.1f, 0.73f)]
	public class CameraFadeTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			ScriptPlayable<CameraFadeMixer> scriptPlayable = ScriptPlayable<CameraFadeMixer>.Create(graph);
			scriptPlayable.SetInputCount(inputCount);
			return scriptPlayable;
		}
	}
}
