namespace AC
{
	public struct CustomToken
	{
		public int ID;

		public string replacementText;

		public CustomToken(int _ID, string _replacementText)
		{
			ID = _ID;
			replacementText = _replacementText;
		}

		public string GetSafeReplacementText()
		{
			return AdvGame.PrepareStringForSaving(replacementText);
		}

		public void SetSafeReplacementText(string safeText)
		{
			replacementText = AdvGame.PrepareStringForLoading(safeText);
		}
	}
}
