using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(MeshRenderer))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_tint_map.html")]
	public class TintMap : MonoBehaviour
	{
		public Texture2D tintMapTexture;

		public Color colorModifier = Color.white;

		public bool disableRenderer = true;

		protected Texture2D actualTexture;

		protected void Awake()
		{
			AssignTexture(tintMapTexture);
			if (disableRenderer)
			{
				GetComponent<MeshRenderer>().enabled = false;
			}
		}

		public Color GetColorData(Vector2 position, float intensity = 1f, float alpha = 1f)
		{
			if (actualTexture != null && intensity > 0f)
			{
				Ray ray = new Ray(new Vector3(position.x, position.y, base.transform.position.z - 0.0005f), Vector3.forward);
				RaycastHit hitInfo;
				if (!Physics.Raycast(ray, out hitInfo, 0.001f))
				{
					return new Color(1f, 1f, 1f, alpha) * colorModifier;
				}
				Vector2 textureCoord = hitInfo.textureCoord;
				if (intensity >= 1f)
				{
					Color pixelBilinear = actualTexture.GetPixelBilinear(textureCoord.x, textureCoord.y);
					pixelBilinear.a = alpha;
					return pixelBilinear * colorModifier;
				}
				Color color = Color.Lerp(Color.white, actualTexture.GetPixelBilinear(textureCoord.x, textureCoord.y) * colorModifier, intensity);
				return new Color(color.r, color.g, color.b, alpha);
			}
			Color color2 = Color.Lerp(Color.white, new Color(1f, 1f, 1f, alpha) * colorModifier, intensity);
			return new Color(color2.r, color2.g, color2.b, alpha);
		}

		protected void AssignTexture(Texture2D newTexture = null)
		{
			if ((bool)GetComponent<MeshRenderer>().material)
			{
				if (newTexture != null)
				{
					GetComponent<MeshRenderer>().material.mainTexture = newTexture;
				}
				actualTexture = (Texture2D)GetComponent<MeshRenderer>().material.mainTexture;
			}
		}
	}
}
