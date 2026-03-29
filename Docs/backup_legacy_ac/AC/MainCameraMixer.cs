using UnityEngine.Playables;

namespace AC
{
	internal sealed class MainCameraMixer : PlayableBehaviour
	{
		private struct ClipInfo
		{
			public _Camera camera;

			public float weight;

			public float shakeIntensity;

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
				MainCamera.ReleaseTimelineOverride();
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
			float num2 = 0f;
			for (int i = 0; i < playable.GetInputCount(); i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				ScriptPlayable<MainCameraPlayableBehaviour> playable2 = (ScriptPlayable<MainCameraPlayableBehaviour>)playable.GetInput(i);
				MainCameraPlayableBehaviour behaviour = playable2.GetBehaviour();
				if (behaviour != null && behaviour.IsValid && playable.GetPlayState() == PlayState.Playing && inputWeight > 0.0001f)
				{
					clipInfo = clipInfo2;
					clipInfo2.camera = behaviour.gameCamera;
					clipInfo2.weight = inputWeight;
					clipInfo2.localTime = playable2.GetTime();
					clipInfo2.duration = playable2.GetDuration();
					clipInfo2.shakeIntensity = behaviour.shakeIntensity;
					if (++num == 2)
					{
						break;
					}
				}
			}
			bool flag = clipInfo2.weight >= 1f || clipInfo2.localTime < clipInfo2.duration / 2.0;
			if (num == 2)
			{
				flag = clipInfo2.localTime > clipInfo.localTime || (!(clipInfo2.localTime < clipInfo.localTime) && clipInfo2.duration >= clipInfo.duration);
			}
			num2 = ((!flag) ? clipInfo.shakeIntensity : clipInfo2.shakeIntensity);
			_Camera camera = ((!flag) ? clipInfo2.camera : clipInfo.camera);
			_Camera camera2 = ((!flag) ? clipInfo.camera : clipInfo2.camera);
			float num3 = ((!flag) ? (1f - clipInfo2.weight) : clipInfo2.weight);
			if (camera2 == null)
			{
				camera2 = camera;
				camera = null;
				num3 = 1f - num3;
			}
			num2 = ((!flag) ? (clipInfo2.shakeIntensity * (1f - num3) + clipInfo.shakeIntensity * num3) : (clipInfo.shakeIntensity * (1f - num3) + clipInfo2.shakeIntensity * num3));
			MainCamera.SetTimelineOverride(camera, camera2, num3, num2);
		}
	}
}
