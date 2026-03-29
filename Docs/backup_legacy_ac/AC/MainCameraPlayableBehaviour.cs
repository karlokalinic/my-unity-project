using UnityEngine.Playables;

namespace AC
{
	internal sealed class MainCameraPlayableBehaviour : PlayableBehaviour
	{
		public _Camera gameCamera;

		public float shakeIntensity;

		public bool IsValid
		{
			get
			{
				return gameCamera != null;
			}
		}
	}
}
