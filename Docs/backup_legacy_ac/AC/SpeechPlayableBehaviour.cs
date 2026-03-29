using System;
using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	[Serializable]
	public class SpeechPlayableBehaviour : PlayableBehaviour
	{
		protected SpeechPlayableData speechPlayableData;

		protected SpeechTrackPlaybackMode speechTrackPlaybackMode;

		protected Char speaker;

		protected bool isPlaying;

		protected int trackInstanceID;

		public Char Speaker
		{
			get
			{
				return speaker;
			}
		}

		public void Init(SpeechPlayableData _speechPlayableData, Char _speaker, SpeechTrackPlaybackMode _speechTrackPlaybackMode, int _trackInstanceID)
		{
			speechPlayableData = _speechPlayableData;
			speaker = _speaker;
			speechTrackPlaybackMode = _speechTrackPlaybackMode;
			trackInstanceID = _trackInstanceID;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			isPlaying = IsValid();
			base.OnBehaviourPlay(playable, info);
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (isPlaying)
			{
				isPlaying = false;
				if (Application.isPlaying)
				{
					string text = speechPlayableData.messageText;
					int language = Options.GetLanguage();
					if (language > 0)
					{
						text = KickStarter.runtimeLanguages.GetTranslation(text, speechPlayableData.lineID, language);
					}
					if (speechTrackPlaybackMode == SpeechTrackPlaybackMode.ClipDuration)
					{
						text += "[hold]";
					}
					KickStarter.dialog.StartDialog(speaker, text, false, speechPlayableData.lineID, false, true);
				}
				else
				{
					ACDebug.Log("Playing speech line with track ID: " + trackInstanceID);
				}
			}
			base.ProcessFrame(playable, info, playerData);
		}

		protected bool IsValid()
		{
			if (speechPlayableData != null && !string.IsNullOrEmpty(speechPlayableData.messageText))
			{
				return true;
			}
			return false;
		}
	}
}
