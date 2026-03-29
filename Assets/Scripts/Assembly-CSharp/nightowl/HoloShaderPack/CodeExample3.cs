using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample3 : MonoBehaviour
	{
		public Material Material;

		public float TargetStrength;

		private void Update()
		{
			Material.SetFloat("_Strength", 1f + CodeExampleHelper.NormalizedTime * TargetStrength);
		}
	}
}
