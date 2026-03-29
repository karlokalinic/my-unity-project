using System;

namespace AC
{
	[Serializable]
	public class JournalPage
	{
		public int lineID = -1;

		public string text = string.Empty;

		public JournalPage()
		{
		}

		public JournalPage(JournalPage journalPage)
		{
			lineID = journalPage.lineID;
			text = journalPage.text;
		}

		public JournalPage(int _lineID, string _text)
		{
			lineID = _lineID;
			text = _text;
		}
	}
}
