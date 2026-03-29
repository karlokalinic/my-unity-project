using System;

namespace AC
{
	[Serializable]
	public class SpeechTag
	{
		public int ID;

		public string label;

		public SpeechTag(int[] idArray)
		{
			ID = 0;
			label = string.Empty;
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

		public SpeechTag(string _label)
		{
			ID = 0;
			label = _label;
		}
	}
}
