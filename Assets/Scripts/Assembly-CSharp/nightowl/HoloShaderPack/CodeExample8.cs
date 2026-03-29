using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample8 : MonoBehaviour
	{
		public Material Material;

		public float TargetStrength;

		private void Update()
		{
			Vector4 vector = Material.GetVector("_ScanLineSettings");
			float num = Time.time % 6f;
			if (num < 2f)
			{
				vector.x = -1f;
			}
			else if (num < 4f)
			{
				vector.x = -1.5f;
			}
			else
			{
				vector.x = -1000f;
			}
			Material.SetVector("_ScanLineSettings", vector);
		}
	}
}
