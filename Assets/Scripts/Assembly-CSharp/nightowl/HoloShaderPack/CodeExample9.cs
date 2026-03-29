using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample9 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			Vector4 vector = Material.GetVector("_ScanLineSettings");
			vector.y = 0.01f + CodeExampleHelper.NormalizedTime * 0.5f;
			Material.SetVector("_ScanLineSettings", vector);
			Material.SetFloat("_ScanLineDistance", Mathf.Sin(Time.time * 0.5f) * 0.5f + 0.5f);
		}
	}
}
