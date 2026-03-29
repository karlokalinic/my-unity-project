using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_marker.html")]
	[AddComponentMenu("Adventure Creator/Navigation/Marker")]
	public class Marker : MonoBehaviour
	{
		protected void Awake()
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null)
			{
				component.enabled = false;
			}
			if (SceneSettings.IsUnity2D())
			{
				base.transform.RotateAround(base.transform.position, Vector3.right, 90f);
				base.transform.RotateAround(base.transform.position, base.transform.right, -90f);
			}
		}
	}
}
