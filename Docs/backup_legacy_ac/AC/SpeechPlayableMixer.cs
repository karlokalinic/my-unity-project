using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	internal sealed class SpeechPlayableMixer : PlayableBehaviour
	{
		public int trackInstanceID;

		public SpeechTrackPlaybackMode playbackMode;

		private Char speaker;

		private bool speakerSet;

		public override void OnGraphStop(Playable playable)
		{
			if (speakerSet && playable.GetInputCount() > 0)
			{
				StopSpeaking();
			}
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			base.ProcessFrame(playable, info, playerData);
			if (!speakerSet)
			{
				SpeechPlayableBehaviour behaviour = ((ScriptPlayable<SpeechPlayableBehaviour>)playable.GetInput(0)).GetBehaviour();
				if (behaviour != null)
				{
					speakerSet = true;
					speaker = behaviour.Speaker;
				}
			}
			for (int i = 0; i < playable.GetInputCount(); i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				if (inputWeight > 0f)
				{
					return;
				}
			}
			if (playbackMode == SpeechTrackPlaybackMode.ClipDuration || !Application.isPlaying)
			{
				StopSpeaking();
			}
		}

		private void StopSpeaking()
		{
			if (Application.isPlaying && KickStarter.dialog != null)
			{
				KickStarter.dialog.EndSpeechByCharacter(speaker);
			}
		}
	}
}
