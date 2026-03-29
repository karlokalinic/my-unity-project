using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample18 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			Vector4 vector = Material.GetVector("_DistortionSettings");
			vector.z = CodeExampleHelper.NormalizedTime * 0.1f;
			Material.SetVector("_DistortionSettings", vector);
		}
	}
}
