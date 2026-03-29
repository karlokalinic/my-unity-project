using System;

namespace AC
{
	[Serializable]
	public class ConversationData : RememberData
	{
		public string _optionStates;

		public string _optionLocks;

		public string _optionChosens;

		public int lastOption;

		public string _optionLabels;

		public string _optionLineIDs;
	}
}
