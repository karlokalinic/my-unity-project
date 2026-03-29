using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	internal sealed class CameraFadeMixer : PlayableBehaviour
	{
		private struct ClipInfo
		{
			public Texture2D overlayTexture;

			public float weight;

			public double localTime;

			public double duration;
		}

		private MainCamera MainCamera
		{
			get
			{
				return KickStarter.mainCamera;
			}
		}

		public override void OnGraphStop(Playable playable)
		{
			if (MainCamera != null)
			{
				MainCamera.ReleaseTimelineFadeOverride();
			}
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			base.ProcessFrame(playable, info, playerData);
			if (MainCamera == null)
			{
				return;
			}
			int num = 0;
			ClipInfo clipInfo = default(ClipInfo);
			ClipInfo clipInfo2 = default(ClipInfo);
			Texture2D texture2D = null;
			for (int i = 0; i < playable.GetInputCount(); i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				ScriptPlayable<CameraFadePlayableBehaviour> playable2 = (ScriptPlayable<CameraFadePlayableBehaviour>)playable.GetInput(i);
				CameraFadePlayableBehaviour behaviour = playable2.GetBehaviour();
				if (behaviour != null && behaviour.IsValid && playable.GetPlayState() == PlayState.Playing && inputWeight > 0.0001f)
				{
					clipInfo = clipInfo2;
					clipInfo2.weight = inputWeight;
					clipInfo2.localTime = playable2.GetTime();
					clipInfo2.duration = playable2.GetDuration();
					clipInfo2.overlayTexture = behaviour.overlayTexture;
					if (++num == 2)
					{
						break;
					}
				}
			}
			texture2D = ((!(clipInfo2.overlayTexture != null)) ? clipInfo.overlayTexture : clipInfo2.overlayTexture);
			float weight = clipInfo2.weight;
			MainCamera.SetTimelineFadeOverride(texture2D, weight);
		}
	}
}
