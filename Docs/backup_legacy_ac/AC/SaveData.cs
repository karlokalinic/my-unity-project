using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class SaveData
	{
		public MainData mainData;

		public List<PlayerData> playerData = new List<PlayerData>();
	}
}
