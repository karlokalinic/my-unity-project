using System.Collections.Generic;

namespace AC
{
	public interface iSaveFileHandler
	{
		string GetDefaultSaveLabel(int saveID);

		void DeleteAll(int profileID);

		bool Delete(SaveFile saveFile);

		void Save(SaveFile saveFile, string dataToSave);

		string Load(SaveFile saveFile, bool doLog);

		List<SaveFile> GatherSaveFiles(int profileID);

		List<SaveFile> GatherImportFiles(int profileID, int boolID, string separateProductName, string separateFilePrefix);

		void SaveScreenshot(SaveFile saveFile);
	}
}
