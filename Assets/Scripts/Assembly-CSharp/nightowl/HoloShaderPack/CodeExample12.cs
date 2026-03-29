using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample12 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			if (Time.time % 2f < 1f)
			{
				HoloShader.EnableNoise(Material, HoloShader.Noise.On);
			}
			else
			{
				HoloShader.EnableNoise(Material, HoloShader.Noise.Off);
			}
		}
	}
}
