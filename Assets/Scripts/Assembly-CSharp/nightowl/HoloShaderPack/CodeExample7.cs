using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample7 : MonoBehaviour
	{
		public Material Material;

		public Color TargetColor;

		private void Update()
		{
			Material.SetColor("_ScanLineColor", CodeExampleHelper.NormalizedTime * TargetColor + (1f - CodeExampleHelper.NormalizedTime) * Color.white);
		}
	}
}
