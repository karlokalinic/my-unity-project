namespace AC
{
	public struct SpeechLog
	{
		public string fullText;

		public string speakerName;

		public int lineID;

		public string textWithRichTextTags;

		public void Clear()
		{
			fullText = string.Empty;
			speakerName = string.Empty;
			lineID = -1;
		}
	}
}
