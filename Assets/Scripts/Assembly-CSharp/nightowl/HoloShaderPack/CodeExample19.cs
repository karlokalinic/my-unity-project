using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample19 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			if (Time.time % 2f < 1f)
			{
				HoloShader.EnableDistortion(Material, HoloShader.Distortion.Distirtion);
			}
			else
			{
				HoloShader.EnableDistortion(Material, HoloShader.Distortion.Off);
			}
		}
	}
}
