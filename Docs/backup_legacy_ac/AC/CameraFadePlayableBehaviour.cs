using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	internal sealed class CameraFadePlayableBehaviour : PlayableBehaviour
	{
		public Texture2D overlayTexture;

		public bool IsValid
		{
			get
			{
				return overlayTexture != null;
			}
		}
	}
}
