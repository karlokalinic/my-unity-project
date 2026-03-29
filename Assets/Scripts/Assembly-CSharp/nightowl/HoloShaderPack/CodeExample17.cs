using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample17 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			Vector4 vector = Material.GetVector("_DistortionSettings");
			vector.x = Mathf.Sin(Time.time) * 0.15f + 0.15f;
			Material.SetVector("_DistortionSettings", vector);
		}
	}
}
