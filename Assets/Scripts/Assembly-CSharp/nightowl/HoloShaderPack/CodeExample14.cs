using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample14 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			Vector4 vector = Material.GetVector("_NoiseFrequency");
			vector.x = Mathf.Sin(Time.time) * 0.01f;
			vector.y = Mathf.Sin(Time.time * 2f) * 0.03f;
			Material.SetVector("_NoiseFrequency", vector);
		}
	}
}
