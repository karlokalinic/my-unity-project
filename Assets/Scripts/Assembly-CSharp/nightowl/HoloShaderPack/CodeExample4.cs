using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample4 : MonoBehaviour
	{
		public Material Material;

		public float TargetStrength;

		private void Update()
		{
			Material.SetFloat("_RimStrength", CodeExampleHelper.NormalizedTime * TargetStrength);
		}
	}
}
