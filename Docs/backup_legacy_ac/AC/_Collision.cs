using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1___collision.html")]
	public class _Collision : MonoBehaviour
	{
		public bool controlsObjectLayer = true;

		public void TurnOn()
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			else
			{
				Collider2D component2 = GetComponent<Collider2D>();
				if (component2 != null)
				{
					component2.enabled = true;
				}
			}
			if (controlsObjectLayer)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
		}

		public void TurnOff()
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			else
			{
				Collider2D component2 = GetComponent<Collider2D>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
			if (controlsObjectLayer)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
		}
	}
}
