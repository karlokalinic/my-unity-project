using System;

namespace AC
{
	[Serializable]
	public class HotspotPrefix : ITranslatable
	{
		public string label;

		public int lineID;

		public HotspotPrefix(string text)
		{
			label = text;
			lineID = -1;
		}

		public string GetTranslatableString(int index)
		{
			return label;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
