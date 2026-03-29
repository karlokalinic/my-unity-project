using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Trigger")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_trigger.html")]
	public class RememberTrigger : Remember
	{
		public AC_OnOff startState;

		private bool loadedData;

		private void Awake()
		{
			if (!loadedData && GameIsPlaying() && (bool)GetComponent<AC_Trigger>())
			{
				if (startState == AC_OnOff.On)
				{
					GetComponent<AC_Trigger>().TurnOn();
				}
				else
				{
					GetComponent<AC_Trigger>().TurnOff();
				}
			}
		}

		public override string SaveData()
		{
			TriggerData triggerData = new TriggerData();
			triggerData.objectID = constantID;
			triggerData.savePrevented = savePrevented;
			if ((bool)GetComponent<Collider>())
			{
				triggerData.isOn = GetComponent<Collider>().enabled;
			}
			else if ((bool)GetComponent<Collider2D>())
			{
				triggerData.isOn = GetComponent<Collider2D>().enabled;
			}
			else
			{
				triggerData.isOn = false;
			}
			return Serializer.SaveScriptData<TriggerData>(triggerData);
		}

		public override void LoadData(string stringData)
		{
			TriggerData triggerData = Serializer.LoadScriptData<TriggerData>(stringData);
			if (triggerData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = triggerData.savePrevented;
			if (!savePrevented)
			{
				if ((bool)GetComponent<Collider>())
				{
					GetComponent<Collider>().enabled = triggerData.isOn;
				}
				else if ((bool)GetComponent<Collider2D>())
				{
					GetComponent<Collider2D>().enabled = triggerData.isOn;
				}
				loadedData = true;
			}
		}
	}
}
