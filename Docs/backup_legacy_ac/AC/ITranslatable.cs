namespace AC
{
	public interface ITranslatable
	{
		string GetTranslatableString(int index);

		int GetTranslationID(int index);
	}
}
