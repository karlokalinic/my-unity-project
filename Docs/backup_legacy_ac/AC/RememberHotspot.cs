using System.Text;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Hotspot")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_hotspot.html")]
	public class RememberHotspot : Remember
	{
		public AC_OnOff startState;

		private bool loadedData;

		private Hotspot ownHotspot;

		private Hotspot OwnHotspot
		{
			get
			{
				if (ownHotspot == null)
				{
					ownHotspot = GetComponent<Hotspot>();
				}
				return ownHotspot;
			}
		}

		private void Awake()
		{
			if (!loadedData && OwnHotspot != null && (bool)KickStarter.settingsManager && GameIsPlaying())
			{
				if (startState == AC_OnOff.On)
				{
					OwnHotspot.TurnOn();
				}
				else
				{
					OwnHotspot.TurnOff();
				}
			}
		}

		public override string SaveData()
		{
			HotspotData hotspotData = new HotspotData();
			hotspotData.objectID = constantID;
			hotspotData.savePrevented = savePrevented;
			if (OwnHotspot != null)
			{
				hotspotData.isOn = OwnHotspot.IsOn();
				hotspotData.buttonStates = ButtonStatesToString(OwnHotspot);
				hotspotData.hotspotName = OwnHotspot.GetName(0);
				hotspotData.displayLineID = OwnHotspot.displayLineID;
			}
			return Serializer.SaveScriptData<HotspotData>(hotspotData);
		}

		public override void LoadData(string stringData)
		{
			HotspotData hotspotData = Serializer.LoadScriptData<HotspotData>(stringData);
			if (hotspotData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = hotspotData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if (hotspotData.isOn)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
			if (OwnHotspot != null)
			{
				if (hotspotData.isOn)
				{
					OwnHotspot.TurnOn();
				}
				else
				{
					OwnHotspot.TurnOff();
				}
				StringToButtonStates(OwnHotspot, hotspotData.buttonStates);
				if (hotspotData.hotspotName != string.Empty)
				{
					OwnHotspot.SetName(hotspotData.hotspotName, hotspotData.displayLineID);
				}
				OwnHotspot.ResetMainIcon();
			}
			loadedData = true;
		}

		private void StringToButtonStates(Hotspot hotspot, string stateString)
		{
			if (string.IsNullOrEmpty(stateString))
			{
				return;
			}
			string[] array = stateString.Split("|"[0]);
			if ((KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive) && hotspot.provideLookInteraction && hotspot.lookButton != null)
			{
				hotspot.lookButton.isDisabled = SetButtonDisabledValue(array[0]);
			}
			if (hotspot.provideUseInteraction && hotspot.useButtons.Count > 0)
			{
				string[] array2 = array[1].Split(","[0]);
				for (int i = 0; i < array2.Length && hotspot.useButtons.Count >= i + 1; i++)
				{
					hotspot.useButtons[i].isDisabled = SetButtonDisabledValue(array2[i]);
				}
			}
			if (hotspot.provideInvInteraction && array.Length > 2 && hotspot.invButtons.Count > 0)
			{
				string[] array3 = array[2].Split(","[0]);
				for (int j = 0; j < array3.Length && hotspot.invButtons.Count >= j + 1; j++)
				{
					hotspot.invButtons[j].isDisabled = SetButtonDisabledValue(array3[j]);
				}
			}
		}

		private string ButtonStatesToString(Hotspot hotspot)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (hotspot.provideLookInteraction)
				{
					stringBuilder.Append(GetButtonDisabledValue(hotspot.lookButton));
				}
				else
				{
					stringBuilder.Append("0");
				}
			}
			else
			{
				stringBuilder.Append("0");
			}
			stringBuilder.Append("|");
			if (hotspot.provideUseInteraction)
			{
				foreach (Button useButton in hotspot.useButtons)
				{
					stringBuilder.Append(GetButtonDisabledValue(useButton));
					if (hotspot.useButtons.IndexOf(useButton) < hotspot.useButtons.Count - 1)
					{
						stringBuilder.Append(",");
					}
				}
				if (hotspot.useButtons.Count == 0)
				{
					stringBuilder.Append("0");
				}
			}
			else
			{
				stringBuilder.Append("0");
			}
			stringBuilder.Append("|");
			if (hotspot.provideInvInteraction)
			{
				foreach (Button invButton in hotspot.invButtons)
				{
					stringBuilder.Append(GetButtonDisabledValue(invButton));
					if (hotspot.invButtons.IndexOf(invButton) < hotspot.invButtons.Count - 1)
					{
						stringBuilder.Append(",");
					}
				}
				if (hotspot.invButtons.Count == 0)
				{
					stringBuilder.Append("0");
				}
			}
			else
			{
				stringBuilder.Append("0");
			}
			return stringBuilder.ToString();
		}

		private string GetButtonDisabledValue(Button button)
		{
			if (button != null && !button.isDisabled)
			{
				return "1";
			}
			return "0";
		}

		private bool SetButtonDisabledValue(string text)
		{
			if (text == "1")
			{
				return false;
			}
			return true;
		}
	}
}
