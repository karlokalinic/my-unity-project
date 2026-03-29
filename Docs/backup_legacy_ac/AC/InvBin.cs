using System;

namespace AC
{
	[Serializable]
	public class InvBin
	{
		public string label;

		public int id;

		public InvBin(int[] idArray)
		{
			id = 0;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			label = "Category " + (id + 1);
		}
	}
}
