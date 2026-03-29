using System;

namespace AC
{
	[Serializable]
	public class HotspotData : RememberData
	{
		public bool isOn;

		public string buttonStates;

		public int displayLineID;

		public string hotspotName;
	}
}
