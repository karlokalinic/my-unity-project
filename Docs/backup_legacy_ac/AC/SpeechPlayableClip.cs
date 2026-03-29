using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[Serializable]
	public class SpeechPlayableClip : PlayableAsset, ITimelineClipAsset
	{
		public Char speaker;

		public SpeechPlayableData speechPlayableData;

		public SpeechTrackPlaybackMode speechTrackPlaybackMode;

		public int trackInstanceID;

		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.None;
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			SpeechPlayableBehaviour template = new SpeechPlayableBehaviour();
			ScriptPlayable<SpeechPlayableBehaviour> scriptPlayable = ScriptPlayable<SpeechPlayableBehaviour>.Create(graph, template);
			SpeechPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
			behaviour.Init(speechPlayableData, speaker, speechTrackPlaybackMode, trackInstanceID);
			return scriptPlayable;
		}

		public string GetDisplayName()
		{
			if (!string.IsNullOrEmpty(speechPlayableData.messageText))
			{
				return speechPlayableData.messageText;
			}
			return "Speech text";
		}
	}
}
