using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample2 : MonoBehaviour
	{
		public Material Material;

		public Color TargetColor;

		private void Update()
		{
			Material.SetColor("_Color", CodeExampleHelper.NormalizedTime * TargetColor + (1f - CodeExampleHelper.NormalizedTime) * Color.white);
		}
	}
}
