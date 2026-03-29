namespace AC
{
	public interface iOptionsFileHandler
	{
		void SaveOptions(int profileID, string dataString, bool showLog);

		string LoadOptions(int profileID, bool showLog);

		void DeleteOptions(int profileID);

		int GetActiveProfile();

		void SetActiveProfile(int profileID);

		bool DoesProfileExist(int profileID);
	}
}
