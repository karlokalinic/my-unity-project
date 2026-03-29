using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample13 : MonoBehaviour
	{
		public Material Material;

		public Color TargetColor;

		private void Update()
		{
			Material.SetColor("_NoiseColor", CodeExampleHelper.NormalizedTime * TargetColor + (1f - CodeExampleHelper.NormalizedTime) * Color.white);
		}
	}
}
