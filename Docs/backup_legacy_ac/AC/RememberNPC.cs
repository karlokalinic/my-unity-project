using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember NPC")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_n_p_c.html")]
	public class RememberNPC : Remember
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
			if (!loadedData && OwnHotspot != null && GetComponent<RememberHotspot>() == null && (bool)KickStarter.settingsManager && GameIsPlaying())
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
			NPCData nPCData = new NPCData();
			nPCData.objectID = constantID;
			nPCData.savePrevented = savePrevented;
			if (OwnHotspot != null)
			{
				nPCData.isOn = OwnHotspot.IsOn();
			}
			nPCData.LocX = base.transform.position.x;
			nPCData.LocY = base.transform.position.y;
			nPCData.LocZ = base.transform.position.z;
			nPCData.RotX = base.transform.eulerAngles.x;
			nPCData.RotY = base.transform.eulerAngles.y;
			nPCData.RotZ = base.transform.eulerAngles.z;
			nPCData.ScaleX = base.transform.localScale.x;
			nPCData.ScaleY = base.transform.localScale.y;
			nPCData.ScaleZ = base.transform.localScale.z;
			if ((bool)GetComponent<NPC>())
			{
				NPC component = GetComponent<NPC>();
				nPCData = component.SaveData(nPCData);
			}
			return Serializer.SaveScriptData<NPCData>(nPCData);
		}

		public override void LoadData(string stringData)
		{
			NPCData nPCData = Serializer.LoadScriptData<NPCData>(stringData);
			if (nPCData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = nPCData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			if (GetComponent<RememberHotspot>() == null && OwnHotspot != null)
			{
				if (nPCData.isOn)
				{
					OwnHotspot.TurnOn();
				}
				else
				{
					OwnHotspot.TurnOff();
				}
			}
			base.transform.position = new Vector3(nPCData.LocX, nPCData.LocY, nPCData.LocZ);
			base.transform.eulerAngles = new Vector3(nPCData.RotX, nPCData.RotY, nPCData.RotZ);
			base.transform.localScale = new Vector3(nPCData.ScaleX, nPCData.ScaleY, nPCData.ScaleZ);
			if ((bool)GetComponent<NPC>())
			{
				NPC component = GetComponent<NPC>();
				component.SetRotation(base.transform.rotation);
				component.LoadData(nPCData);
			}
			loadedData = true;
		}
	}
}
