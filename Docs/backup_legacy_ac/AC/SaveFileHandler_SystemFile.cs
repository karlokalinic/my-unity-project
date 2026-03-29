using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AC
{
	public class SaveFileHandler_SystemFile : iSaveFileHandler
	{
		public string GetDefaultSaveLabel(int saveID)
		{
			string text = "Save " + saveID;
			if (saveID == 0)
			{
				text = "Autosave";
			}
			return text + GetTimeString(DateTime.Now);
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
			if (!string.IsNullOrEmpty(fileName))
			{
				FileInfo fileInfo = new FileInfo(fileName);
				if (fileInfo.Exists)
				{
					fileInfo.Delete();
					if (KickStarter.settingsManager.takeSaveScreenshots)
					{
						DeleteScreenshot(saveFile.screenshotFilename);
					}
					ACDebug.Log("File deleted: " + fileName);
					return true;
				}
			}
			return false;
		}

		public void Save(SaveFile saveFile, string dataToSave)
		{
			string text = GetSaveDirectory(string.Empty) + Path.DirectorySeparatorChar + GetSaveFilename(saveFile.saveID, saveFile.profileID, string.Empty);
			bool wasSuccesful = false;
			try
			{
				FileInfo fileInfo = new FileInfo(text);
				StreamWriter streamWriter;
				if (!fileInfo.Exists)
				{
					streamWriter = fileInfo.CreateText();
				}
				else
				{
					fileInfo.Delete();
					streamWriter = fileInfo.CreateText();
				}
				streamWriter.Write(dataToSave);
				streamWriter.Close();
				ACDebug.Log("File written: " + text);
				wasSuccesful = true;
			}
			catch (Exception ex)
			{
				ACDebug.LogWarning("Could not save data to file '" + text + "'. Exception: " + ex);
			}
			KickStarter.saveSystem.OnFinishSaveRequest(saveFile, wasSuccesful);
		}

		public string Load(SaveFile saveFile, bool doLog)
		{
			string text = string.Empty;
			if (File.Exists(saveFile.fileName))
			{
				StreamReader streamReader = File.OpenText(saveFile.fileName);
				string text2 = streamReader.ReadToEnd();
				streamReader.Close();
				text = text2;
			}
			if (doLog && !string.IsNullOrEmpty(text))
			{
				ACDebug.Log("File read: " + saveFile.fileName);
			}
			return text;
		}

		public List<SaveFile> GatherSaveFiles(int profileID)
		{
			return GatherSaveFiles(profileID, false, -1, string.Empty, string.Empty);
		}

		public List<SaveFile> GatherImportFiles(int profileID, int boolID, string separateProjectName, string separateFilePrefix)
		{
			if (!string.IsNullOrEmpty(separateProjectName) && !string.IsNullOrEmpty(separateFilePrefix))
			{
				return GatherSaveFiles(profileID, true, boolID, separateProjectName, separateFilePrefix);
			}
			return null;
		}

		protected List<SaveFile> GatherSaveFiles(int profileID, bool isImport, int boolID, string separateProductName, string separateFilePrefix)
		{
			List<SaveFile> list = new List<SaveFile>();
			string saveDirectory = GetSaveDirectory(separateProductName);
			string text = ((!isImport) ? KickStarter.settingsManager.SavePrefix : separateFilePrefix);
			for (int i = 0; i < 50; i++)
			{
				string text2 = text + SaveSystem.GenerateSaveSuffix(i, profileID);
				string text3 = text2 + SaveSystem.GetSaveExtension();
				string text4 = saveDirectory + Path.DirectorySeparatorChar + text3;
				if (!File.Exists(text4))
				{
					continue;
				}
				if (isImport && boolID >= 0)
				{
					string fileData = LoadFile(text4, false);
					if (!KickStarter.saveSystem.DoImportCheck(fileData, boolID))
					{
						continue;
					}
				}
				int updatedTime = 0;
				bool flag = false;
				string text5 = ((!isImport) ? "Save " : "Import ") + i;
				if (i == 0)
				{
					text5 = "Autosave";
					flag = true;
				}
				if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(saveDirectory);
					FileInfo[] files = directoryInfo.GetFiles(text3);
					if (files != null && files.Length > 0)
					{
						if (!flag)
						{
							updatedTime = (int)(files[0].LastWriteTime - new DateTime(2015, 1, 1)).TotalSeconds;
						}
						text5 += GetTimeString(files[0].LastWriteTime);
					}
				}
				Texture2D screenShot = null;
				string text6 = string.Empty;
				if (KickStarter.settingsManager.takeSaveScreenshots)
				{
					text6 = saveDirectory + Path.DirectorySeparatorChar + text2 + ".jpg";
					screenShot = LoadScreenshot(text6);
				}
				list.Add(new SaveFile(i, profileID, text5, text4, flag, screenShot, text6, updatedTime));
			}
			return list;
		}

		public void SaveScreenshot(SaveFile saveFile)
		{
			if (saveFile.screenShot != null)
			{
				string text = GetSaveDirectory(string.Empty) + Path.DirectorySeparatorChar + GetSaveFilename(saveFile.saveID, saveFile.profileID, ".jpg");
				byte[] bytes = saveFile.screenShot.EncodeToJPG();
				File.WriteAllBytes(text, bytes);
				ACDebug.Log("Saved screenshot: " + text);
			}
			else
			{
				ACDebug.LogWarning("Cannot save screenshot - SaveFile's screenshot variable is null.");
			}
		}

		protected void DeleteScreenshot(string sceenshotFilename)
		{
			if (File.Exists(sceenshotFilename))
			{
				File.Delete(sceenshotFilename);
			}
		}

		protected Texture2D LoadScreenshot(string fileName)
		{
			if (File.Exists(fileName))
			{
				byte[] data = File.ReadAllBytes(fileName);
				Texture2D texture2D = new Texture2D(ACScreen.width, ACScreen.height, TextureFormat.RGB24, false);
				texture2D.LoadImage(data);
				return texture2D;
			}
			return null;
		}

		protected string GetSaveFilename(int saveID, int profileID = -1, string extensionOverride = "")
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID();
			}
			string text = (string.IsNullOrEmpty(extensionOverride) ? SaveSystem.GetSaveExtension() : extensionOverride);
			return KickStarter.settingsManager.SavePrefix + SaveSystem.GenerateSaveSuffix(saveID, profileID) + text;
		}

		protected string GetSaveDirectory(string separateProjectName = "")
		{
			string persistentDataPath = Application.persistentDataPath;
			if (!string.IsNullOrEmpty(separateProjectName))
			{
				string[] array = persistentDataPath.Split('/');
				string oldValue = array[array.Length - 1];
				return persistentDataPath.Replace(oldValue, separateProjectName);
			}
			return persistentDataPath;
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

		protected string LoadFile(string fullFilename, bool doLog = true)
		{
			string text = string.Empty;
			if (File.Exists(fullFilename))
			{
				StreamReader streamReader = File.OpenText(fullFilename);
				string text2 = streamReader.ReadToEnd();
				streamReader.Close();
				text = text2;
			}
			if (text != string.Empty && doLog)
			{
				ACDebug.Log("File Read: " + fullFilename);
			}
			return text;
		}
	}
}
