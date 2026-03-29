using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class CodeExample1 : MonoBehaviour
	{
		public Material Material;

		public Texture Texture;

		private void Update()
		{
			if (Time.time % 2f < 1f)
			{
				Material.SetTexture("_MainTex", Texture);
			}
			else
			{
				Material.SetTexture("_MainTex", null);
			}
		}
	}
}
