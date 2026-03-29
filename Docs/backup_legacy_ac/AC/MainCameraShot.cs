using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	public sealed class MainCameraShot : PlayableAsset
	{
		public ExposedReference<_Camera> gameCamera;

		public float shakeIntensity;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			ScriptPlayable<MainCameraPlayableBehaviour> scriptPlayable = ScriptPlayable<MainCameraPlayableBehaviour>.Create(graph);
			scriptPlayable.GetBehaviour().gameCamera = gameCamera.Resolve(graph.GetResolver());
			scriptPlayable.GetBehaviour().shakeIntensity = shakeIntensity;
			return scriptPlayable;
		}
	}
}
