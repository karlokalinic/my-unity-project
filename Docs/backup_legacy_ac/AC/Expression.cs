using System;

namespace AC
{
	[Serializable]
	public class Expression
	{
		public int ID;

		public string label;

		public CursorIconBase portraitIcon = new CursorIconBase();

		public Expression(int[] idArray)
		{
			ID = 0;
			portraitIcon = new CursorIconBase();
			label = "New expression";
			if (idArray == null || idArray.Length <= 0)
			{
				return;
			}
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}
	}
}
