using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class SaveFileHandler_PlayerPrefs : iSaveFileHandler
	{
		private string screenshotKey = "_screenshot";

		public string GetDefaultSaveLabel(int saveID)
		{
			string result = "Save " + saveID;
			if (saveID == 0)
			{
				result = "Autosave";
			}
			return result;
		}

		public void DeleteAll(int profileID)
		{
			List<SaveFile> list = GatherSaveFiles(profileID);
			foreach (SaveFile item in list)
			{
				Delete(item);
			}
		}

		public bool Delete(SaveFile saveFile)
		{
			string fileName = saveFile.fileName;
			if (PlayerPrefs.HasKey(fileName))
			{
				PlayerPrefs.DeleteKey(fileName);
				ACDebug.Log("PlayerPrefs key deleted: " + fileName);
				if (KickStarter.settingsManager.takeSaveScreenshots && PlayerPrefs.HasKey(fileName + screenshotKey))
				{
					PlayerPrefs.DeleteKey(fileName + screenshotKey);
				}
				return true;
			}
			return false;
		}

		public void Save(SaveFile saveFile, string dataToSave)
		{
			string saveFilename = GetSaveFilename(saveFile.saveID, saveFile.profileID);
			bool flag = false;
			try
			{
				PlayerPrefs.SetString(saveFilename, dataToSave);
				ACDebug.Log("PlayerPrefs key written: " + saveFilename);
				flag = true;
			}
			catch (Exception ex)
			{
				ACDebug.LogWarning("Could not save PlayerPrefs data under key " + saveFilename + ". Exception: " + ex);
			}
			if (flag)
			{
				string text = saveFilename + "_timestamp";
				try
				{
					DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0).ToUniversalTime();
					string value = ((int)(DateTime.UtcNow - dateTime).TotalSeconds/*cast due to .constrained prefix*/).ToString();
					PlayerPrefs.SetString(text, value);
				}
				catch (Exception ex2)
				{
					ACDebug.LogWarning("Could not save PlayerPrefs data under key " + text + ". Exception: " + ex2);
				}
			}
			KickStarter.saveSystem.OnFinishSaveRequest(saveFile, flag);
		}

		public string Load(SaveFile saveFile, bool doLog)
		{
			string fileName = saveFile.fileName;
			string text = PlayerPrefs.GetString(fileName, string.Empty);
			if (doLog && !string.IsNullOrEmpty(text))
			{
				ACDebug.Log("PlayerPrefs key read: " + fileName);
			}
			return text;
		}

		public List<SaveFile> GatherSaveFiles(int profileID)
		{
			return GatherSaveFiles(profileID, false, -1, string.Empty);
		}

		public List<SaveFile> GatherImportFiles(int profileID, int boolID, string separateProductName, string separateFilePrefix)
		{
			if (!string.IsNullOrEmpty(separateProductName) && !string.IsNullOrEmpty(separateFilePrefix))
			{
				return GatherSaveFiles(profileID, true, boolID, separateFilePrefix);
			}
			return null;
		}

		protected List<SaveFile> GatherSaveFiles(int profileID, bool isImport, int boolID, string separateFilePrefix)
		{
			List<SaveFile> list = new List<SaveFile>();
			for (int i = 0; i < 50; i++)
			{
				bool flag = false;
				string text = ((!isImport) ? GetSaveFilename(i, profileID) : GetImportFilename(i, separateFilePrefix, profileID));
				if (!PlayerPrefs.HasKey(text))
				{
					continue;
				}
				string text2 = "Save " + i;
				if (i == 0)
				{
					text2 = "Autosave";
					flag = true;
				}
				Texture2D texture2D = null;
				if (KickStarter.settingsManager.takeSaveScreenshots && PlayerPrefs.HasKey(text + screenshotKey) && KickStarter.saveSystem != null)
				{
					try
					{
						string text3 = PlayerPrefs.GetString(text + screenshotKey);
						if (!string.IsNullOrEmpty(text3))
						{
							byte[] array = Convert.FromBase64String(text3);
							if (array != null)
							{
								texture2D = new Texture2D(KickStarter.saveSystem.ScreenshotWidth, KickStarter.saveSystem.ScreenshotHeight, TextureFormat.RGB24, false);
								texture2D.LoadImage(array);
								texture2D.Apply();
							}
						}
					}
					catch (Exception ex)
					{
						ACDebug.LogWarning("Could not load PlayerPrefs data from key " + text + screenshotKey + ". Exception: " + ex);
					}
				}
				int result = 0;
				if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
				{
					string key = text + "_timestamp";
					if (PlayerPrefs.HasKey(key))
					{
						string text4 = PlayerPrefs.GetString(key);
						if (!string.IsNullOrEmpty(text4) && int.TryParse(text4, out result) && !flag)
						{
							DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0).ToUniversalTime().AddSeconds(result);
							text2 += GetTimeString(dateTime);
						}
					}
				}
				list.Add(new SaveFile(i, profileID, text2, text, flag, texture2D, string.Empty, result));
			}
			return list;
		}

		public void SaveScreenshot(SaveFile saveFile)
		{
			string text = GetSaveFilename(saveFile.saveID, saveFile.profileID) + screenshotKey;
			try
			{
				byte[] inArray = saveFile.screenShot.EncodeToJPG();
				string value = Convert.ToBase64String(inArray);
				PlayerPrefs.SetString(text, value);
				ACDebug.Log("PlayerPrefs key written: " + text);
			}
			catch (Exception ex)
			{
				ACDebug.LogWarning("Could not save PlayerPrefs data under key " + text + ". Exception: " + ex);
			}
		}

		protected string GetSaveFilename(int saveID, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID();
			}
			return KickStarter.settingsManager.SavePrefix + SaveSystem.GenerateSaveSuffix(saveID, profileID);
		}

		protected string GetImportFilename(int saveID, string filePrefix, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID();
			}
			return filePrefix + SaveSystem.GenerateSaveSuffix(saveID, profileID);
		}

		protected string GetTimeString(DateTime dateTime)
		{
			if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
			{
				if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.CustomFormat)
				{
					string text = dateTime.ToString(KickStarter.settingsManager.customSaveFormat);
					return " (" + text + ")";
				}
				string text2 = dateTime.ToShortDateString();
				if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.TimeAndDate)
				{
					text2 = text2 + " " + dateTime.ToShortTimeString();
				}
				return " (" + text2 + ")";
			}
			return string.Empty;
		}
	}
}
