using System;

namespace AC
{
	[Serializable]
	public class PlayerPrefab
	{
		public Player playerOb;

		public int ID;

		public bool isDefault;

		public PlayerPrefab(int[] idArray)
		{
			ID = 0;
			playerOb = null;
			if (idArray.Length > 0)
			{
				isDefault = false;
				foreach (int num in idArray)
				{
					if (ID == num)
					{
						ID++;
					}
				}
			}
			else
			{
				isDefault = true;
			}
		}
	}
}
