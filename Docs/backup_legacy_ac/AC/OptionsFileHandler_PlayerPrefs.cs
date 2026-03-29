using UnityEngine;

namespace AC
{
	public class OptionsFileHandler_PlayerPrefs : iOptionsFileHandler
	{
		private const string activeProfileKey = "AC_ActiveProfile";

		public void SaveOptions(int profileID, string dataString, bool showLog)
		{
			string prefKeyName = GetPrefKeyName(profileID);
			PlayerPrefs.SetString(prefKeyName, dataString);
			if (showLog)
			{
				ACDebug.Log("PlayerPrefs Key '" + prefKeyName + "' saved");
			}
		}

		public string LoadOptions(int profileID, bool showLog)
		{
			string prefKeyName = GetPrefKeyName(profileID);
			string text = PlayerPrefs.GetString(prefKeyName);
			if (!string.IsNullOrEmpty(text) && showLog)
			{
				ACDebug.Log("PlayerPrefs Key '" + prefKeyName + "' loaded");
			}
			return text;
		}

		public void DeleteOptions(int profileID)
		{
			string prefKeyName = GetPrefKeyName(profileID);
			if (PlayerPrefs.HasKey(prefKeyName))
			{
				PlayerPrefs.DeleteKey(prefKeyName);
				ACDebug.Log("PlayerPrefs Key '" + prefKeyName + "' deleted");
			}
		}

		public int GetActiveProfile()
		{
			return PlayerPrefs.GetInt("AC_ActiveProfile", 0);
		}

		public void SetActiveProfile(int profileID)
		{
			PlayerPrefs.SetInt("AC_ActiveProfile", profileID);
		}

		public bool DoesProfileExist(int profileID)
		{
			string prefKeyName = GetPrefKeyName(profileID);
			return PlayerPrefs.HasKey(prefKeyName);
		}

		private string GetPrefKeyName(int profileID)
		{
			string text = "Profile";
			if (AdvGame.GetReferences().settingsManager != null && AdvGame.GetReferences().settingsManager.saveFileName != string.Empty)
			{
				text = AdvGame.GetReferences().settingsManager.saveFileName;
				text = text.Replace(" ", "_");
			}
			return "AC_" + text + "_" + profileID;
		}
	}
}
