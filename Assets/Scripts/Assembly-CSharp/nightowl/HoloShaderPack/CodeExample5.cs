using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample5 : MonoBehaviour
	{
		public Material Material;

		public float TargetStrength;

		private void Update()
		{
			if (CodeExampleHelper.NormalizedTime * TargetStrength > 1f)
			{
				Material.EnableKeyword("RIM_ON");
				Material.DisableKeyword("RIM_ON_INVERT");
				Material.DisableKeyword("RIM_OFF");
			}
			else
			{
				Material.EnableKeyword("RIM_OFF");
				Material.DisableKeyword("RIM_ON");
				Material.DisableKeyword("RIM_ON_INVERT");
			}
			Material.SetFloat("_RimStrength", CodeExampleHelper.NormalizedTime * TargetStrength);
		}
	}
}
