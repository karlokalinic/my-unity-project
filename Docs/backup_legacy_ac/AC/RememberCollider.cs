using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Collider")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_collider.html")]
	public class RememberCollider : Remember
	{
		public AC_OnOff startState;

		private bool loadedData;

		private void Awake()
		{
			if (!loadedData && (bool)KickStarter.settingsManager && GameIsPlaying())
			{
				bool flag = startState == AC_OnOff.On;
				if ((bool)GetComponent<Collider>())
				{
					GetComponent<Collider>().enabled = flag;
				}
				else if ((bool)GetComponent<Collider2D>())
				{
					GetComponent<Collider2D>().enabled = flag;
				}
			}
		}

		public override string SaveData()
		{
			ColliderData colliderData = new ColliderData();
			colliderData.objectID = constantID;
			colliderData.savePrevented = savePrevented;
			colliderData.isOn = false;
			if ((bool)GetComponent<Collider>())
			{
				colliderData.isOn = GetComponent<Collider>().enabled;
			}
			else if ((bool)GetComponent<Collider2D>())
			{
				colliderData.isOn = GetComponent<Collider2D>().enabled;
			}
			return Serializer.SaveScriptData<ColliderData>(colliderData);
		}

		public override void LoadData(string stringData)
		{
			ColliderData colliderData = Serializer.LoadScriptData<ColliderData>(stringData);
			if (colliderData == null)
			{
				loadedData = false;
				return;
			}
			base.SavePrevented = colliderData.savePrevented;
			if (!savePrevented)
			{
				if ((bool)GetComponent<Collider>())
				{
					GetComponent<Collider>().enabled = colliderData.isOn;
				}
				else if ((bool)GetComponent<Collider2D>())
				{
					GetComponent<Collider2D>().enabled = colliderData.isOn;
				}
				loadedData = true;
			}
		}
	}
}
