using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{
	[TrackColor(0.9f, 0.4f, 0.9f)]
	[TrackClipType(typeof(SpeechPlayableClip))]
	public class SpeechTrack : TrackAsset, ITranslatable
	{
		public bool isPlayerLine;

		public GameObject speakerObject;

		public int speakerConstantID;

		public SpeechTrackPlaybackMode playbackMode;

		protected Char SpeakerPrefab
		{
			get
			{
				if (speakerObject != null)
				{
					return speakerObject.GetComponent<Char>();
				}
				return null;
			}
		}

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			foreach (TimelineClip clip in GetClips())
			{
				SpeechPlayableClip speechPlayableClip = (SpeechPlayableClip)clip.asset;
				clip.displayName = speechPlayableClip.GetDisplayName();
				Char speaker = null;
				if (Application.isPlaying)
				{
					if (isPlayerLine)
					{
						speaker = KickStarter.player;
					}
					else if (speakerConstantID != 0)
					{
						speaker = Serializer.returnComponent<Char>(speakerConstantID);
					}
				}
				else if (isPlayerLine)
				{
					if (KickStarter.settingsManager != null)
					{
						speaker = KickStarter.settingsManager.GetDefaultPlayer(false);
					}
				}
				else
				{
					speaker = SpeakerPrefab;
				}
				speechPlayableClip.speechTrackPlaybackMode = playbackMode;
				speechPlayableClip.speaker = speaker;
				speechPlayableClip.trackInstanceID = GetInstanceID();
			}
			ScriptPlayable<SpeechPlayableMixer> scriptPlayable = ScriptPlayable<SpeechPlayableMixer>.Create(graph);
			scriptPlayable.SetInputCount(inputCount);
			scriptPlayable.GetBehaviour().trackInstanceID = GetInstanceID();
			scriptPlayable.GetBehaviour().playbackMode = playbackMode;
			return scriptPlayable;
		}

		public string GetTranslatableString(int index)
		{
			return GetClip(index).speechPlayableData.messageText;
		}

		public int GetTranslationID(int index)
		{
			return GetClip(index).speechPlayableData.lineID;
		}

		protected SpeechPlayableClip[] GetClipsArray()
		{
			List<SpeechPlayableClip> list = new List<SpeechPlayableClip>();
			IEnumerable<TimelineClip> enumerable = GetClips();
			foreach (TimelineClip item in enumerable)
			{
				if (item != null && item.asset is SpeechPlayableClip)
				{
					list.Add(item.asset as SpeechPlayableClip);
				}
			}
			return list.ToArray();
		}

		protected SpeechPlayableClip GetClip(int index)
		{
			return GetClipsArray()[index];
		}
	}
}
