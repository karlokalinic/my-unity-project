using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample6 : MonoBehaviour
	{
		public Material Material;

		private void Update()
		{
			if (Time.time % 2f < 1f)
			{
				HoloShader.EnableScanline(Material, HoloShader.Scanline.World);
			}
			else
			{
				HoloShader.EnableScanline(Material, HoloShader.Scanline.Off);
			}
		}
	}
}
