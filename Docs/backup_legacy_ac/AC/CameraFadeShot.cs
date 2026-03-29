using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	public sealed class CameraFadeShot : PlayableAsset
	{
		public Texture2D overlayTexture;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			ScriptPlayable<CameraFadePlayableBehaviour> scriptPlayable = ScriptPlayable<CameraFadePlayableBehaviour>.Create(graph);
			scriptPlayable.GetBehaviour().overlayTexture = overlayTexture;
			return scriptPlayable;
		}
	}
}
