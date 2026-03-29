using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Light))]
	[AddComponentMenu("Adventure Creator/Misc/Light switch")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_light_switch.html")]
	public class LightSwitch : MonoBehaviour
	{
		public bool enableOnStart;

		protected Light _light;

		protected void Awake()
		{
			Switch(enableOnStart);
		}

		public void TurnOn()
		{
			Switch(true);
		}

		public void TurnOff()
		{
			Switch(false);
		}

		protected void Switch(bool turnOn)
		{
			if (_light == null)
			{
				_light = GetComponent<Light>();
			}
			_light.enabled = turnOn;
		}
	}
}
