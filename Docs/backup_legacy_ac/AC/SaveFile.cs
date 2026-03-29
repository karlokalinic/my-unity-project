using UnityEngine;

namespace AC
{
	public class SaveFile
	{
		public int saveID;

		public int profileID;

		public string label;

		public Texture2D screenShot;

		public string fileName;

		public string screenshotFilename;

		public bool isAutoSave;

		public int updatedTime;

		public SaveFile(int _saveID, int _profileID, string _label, string _fileName, bool _isAutoSave, Texture2D _screenShot, string _screenshotFilename, int _updatedTime = 0)
		{
			saveID = _saveID;
			profileID = _profileID;
			label = _label;
			fileName = _fileName;
			isAutoSave = _isAutoSave;
			screenShot = _screenShot;
			screenshotFilename = _screenshotFilename;
			if (_updatedTime > 0)
			{
				updatedTime = 200000000 - _updatedTime;
			}
			else
			{
				updatedTime = 0;
			}
		}

		public SaveFile(SaveFile _saveFile)
		{
			saveID = _saveFile.saveID;
			profileID = _saveFile.profileID;
			label = _saveFile.label;
			screenShot = _saveFile.screenShot;
			screenshotFilename = _saveFile.screenshotFilename;
			fileName = _saveFile.fileName;
			isAutoSave = _saveFile.isAutoSave;
			updatedTime = _saveFile.updatedTime;
		}

		public void SetLabel(string _label)
		{
			label = AdvGame.PrepareStringForLoading(_label);
		}

		public string GetSafeLabel()
		{
			return AdvGame.PrepareStringForSaving(label);
		}
	}
}
