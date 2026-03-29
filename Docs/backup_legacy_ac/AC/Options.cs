using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_options.html")]
	public class Options : MonoBehaviour
	{
		public static OptionsData optionsData;

		public static int maxProfiles = 50;

		protected static iOptionsFileHandler optionsFileHandlerOverride;

		public static iOptionsFileHandler OptionsFileHandler
		{
			get
			{
				if (optionsFileHandlerOverride != null)
				{
					return optionsFileHandlerOverride;
				}
				return new OptionsFileHandler_PlayerPrefs();
			}
			set
			{
				optionsFileHandlerOverride = value;
				LoadPrefs();
			}
		}

		public void OnStart()
		{
			if (!(KickStarter.settingsManager == null))
			{
				LoadPrefs();
				KickStarter.runtimeLanguages.LoadAssetBundle(GetVoiceLanguage());
				if (!KickStarter.settingsManager.IsInLoadingScene())
				{
					AfterLoad();
				}
			}
		}

		public static void SaveDefaultPrefs(OptionsData defaultOptionsData)
		{
			SavePrefsToID(0, defaultOptionsData);
		}

		public static OptionsData LoadDefaultPrefs()
		{
			return LoadPrefsFromID(0, false, false);
		}

		public static void DeleteDefaultProfile()
		{
			DeleteProfilePrefs(0);
		}

		public static void SavePrefs(bool updateVariables = true)
		{
			if (Application.isPlaying && updateVariables)
			{
				GlobalVariables.DownloadAll();
				optionsData.linkedVariables = SaveSystem.CreateVariablesData(KickStarter.runtimeVariables.globalVars, true, VariableLocation.Global);
			}
			SavePrefsToID(GetActiveProfileID(), null, true);
			if (Application.isPlaying)
			{
				KickStarter.options.CustomSaveOptionsHook();
			}
		}

		public static void SavePrefsToID(int ID, OptionsData _optionsData = null, bool showLog = false)
		{
			if (_optionsData == null)
			{
				_optionsData = optionsData;
			}
			string text = Serializer.SerializeObject<OptionsData>(_optionsData, true, SaveSystem.OptionsFileFormatHandler);
			if (!string.IsNullOrEmpty(text))
			{
				OptionsFileHandler.SaveOptions(ID, text, showLog);
			}
		}

		public static void LoadPrefs()
		{
			if (Application.isPlaying)
			{
				KickStarter.options.CustomLoadOptionsHook();
			}
			optionsData = LoadPrefsFromID(GetActiveProfileID(), Application.isPlaying);
			if (optionsData == null)
			{
				ACDebug.LogWarning("No Options Data found!");
			}
			else
			{
				int num = ((!Application.isPlaying) ? AdvGame.GetReferences().speechManager.languages.Count : KickStarter.runtimeLanguages.Languages.Count);
				if (optionsData.language >= num)
				{
					if (num != 0)
					{
						ACDebug.LogWarning("Language set to an invalid index - reverting to original language.");
					}
					optionsData.language = 0;
					SavePrefs(false);
				}
				if (optionsData.voiceLanguage >= num && KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
				{
					if (num != 0)
					{
						ACDebug.LogWarning("Voice language set to an invalid index - reverting to original language.");
					}
					optionsData.voiceLanguage = 0;
					SavePrefs(false);
				}
				if (KickStarter.speechManager != null && KickStarter.speechManager.ignoreOriginalText && KickStarter.speechManager.languages.Count > 1)
				{
					if (optionsData.language == 0)
					{
						optionsData.language = 1;
						SavePrefs(false);
					}
					if (optionsData.voiceLanguage == 0 && KickStarter.speechManager.separateVoiceAndTextLanguages)
					{
						optionsData.voiceLanguage = 1;
						SavePrefs(false);
					}
				}
			}
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles();
				KickStarter.playerMenus.RecalculateAll();
			}
		}

		public static OptionsData LoadPrefsFromID(int profileID, bool showLog = false, bool doSave = true)
		{
			if (DoesProfileIDExist(profileID))
			{
				string text = OptionsFileHandler.LoadOptions(profileID, showLog);
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						return Serializer.DeserializeOptionsData(text);
					}
					catch (Exception ex)
					{
						ACDebug.LogWarning("Error retrieving OptionsData for profile #" + profileID + " - rebuilding..\nException: " + ex);
						OptionsData result = new OptionsData(profileID);
						if (KickStarter.settingsManager != null)
						{
							result = GenerateDefaultOptionsData(profileID);
						}
						SavePrefsToID(profileID, result);
						return result;
					}
				}
			}
			if (KickStarter.settingsManager == null)
			{
				return null;
			}
			OptionsData result2 = GenerateDefaultOptionsData(profileID);
			if (doSave)
			{
				optionsData = result2;
				SavePrefs();
			}
			return result2;
		}

		private static OptionsData GenerateDefaultOptionsData(int profileID)
		{
			return new OptionsData(KickStarter.settingsManager.defaultLanguage, KickStarter.settingsManager.defaultVoiceLanguage, KickStarter.settingsManager.defaultShowSubtitles, KickStarter.settingsManager.defaultSfxVolume, KickStarter.settingsManager.defaultMusicVolume, KickStarter.settingsManager.defaultSpeechVolume, profileID);
		}

		public bool SwitchProfile(int index, bool includeActive)
		{
			if (KickStarter.settingsManager.useProfiles)
			{
				int profileID = ProfileIndexToID(index, includeActive);
				if (DoesProfileIDExist(profileID))
				{
					return SwitchProfileID(profileID);
				}
				ACDebug.Log("Profile switch failed - " + index + " doesn't exist");
			}
			return false;
		}

		public int ProfileIndexToID(int index, bool includeActive = true)
		{
			for (int i = 0; i < maxProfiles; i++)
			{
				if (DoesProfileIDExist(i) && (includeActive || i != GetActiveProfileID()))
				{
					index--;
				}
				if (index < 0)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetActiveProfileID()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.useProfiles)
			{
				return OptionsFileHandler.GetActiveProfile();
			}
			return 0;
		}

		public static void SetActiveProfileID(int profileID)
		{
			OptionsFileHandler.SetActiveProfile(profileID);
		}

		protected int FindFirstEmptyProfileID()
		{
			for (int i = 0; i < maxProfiles; i++)
			{
				if (!DoesProfileIDExist(i))
				{
					return i;
				}
			}
			return 0;
		}

		public int CreateProfile(string _label = "")
		{
			int num = FindFirstEmptyProfileID();
			OptionsData optionsData = new OptionsData(Options.optionsData, num);
			if (!string.IsNullOrEmpty(_label))
			{
				optionsData.label = _label;
			}
			Options.optionsData = optionsData;
			SetActiveProfileID(num);
			SavePrefs();
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles();
				KickStarter.playerMenus.RecalculateAll();
			}
			return num;
		}

		public void RenameProfile(string newProfileLabel, int profileIndex = -2, bool includeActive = true)
		{
			if (!KickStarter.settingsManager.useProfiles || string.IsNullOrEmpty(newProfileLabel))
			{
				return;
			}
			int num = KickStarter.options.ProfileIndexToID(profileIndex, includeActive);
			if (num == -1)
			{
				ACDebug.LogWarning("Invalid profile index: " + profileIndex + " - nothing to delete!");
				return;
			}
			if (profileIndex == -2)
			{
				num = GetActiveProfileID();
			}
			RenameProfileID(newProfileLabel, num);
		}

		public void RenameProfileID(string newProfileLabel, int profileID)
		{
			if (!KickStarter.settingsManager.useProfiles || string.IsNullOrEmpty(newProfileLabel))
			{
				return;
			}
			if (profileID == GetActiveProfileID())
			{
				Options.optionsData.label = newProfileLabel;
				SavePrefs();
			}
			else
			{
				if (!DoesProfileIDExist(profileID))
				{
					ACDebug.LogWarning("Cannot rename profile " + profileID + " as it does not exist!");
					return;
				}
				OptionsData optionsData = LoadPrefsFromID(profileID);
				optionsData.label = newProfileLabel;
				SavePrefsToID(profileID, optionsData, true);
			}
			KickStarter.playerMenus.RecalculateAll();
		}

		public string GetProfileName(int index = -1, bool includeActive = true)
		{
			if (index == -1 || !KickStarter.settingsManager.useProfiles)
			{
				if (optionsData == null)
				{
					LoadPrefs();
				}
				return optionsData.label;
			}
			int profileID = KickStarter.options.ProfileIndexToID(index, includeActive);
			return GetProfileIDName(profileID);
		}

		public string GetProfileIDName(int profileID)
		{
			if (!KickStarter.settingsManager.useProfiles)
			{
				if (Options.optionsData == null)
				{
					LoadPrefs();
				}
				return Options.optionsData.label;
			}
			if (DoesProfileIDExist(profileID))
			{
				OptionsData optionsData = LoadPrefsFromID(profileID, false, false);
				return optionsData.label;
			}
			return string.Empty;
		}

		public int GetNumProfiles()
		{
			if (KickStarter.settingsManager.useProfiles)
			{
				int num = 0;
				for (int i = 0; i < maxProfiles; i++)
				{
					if (DoesProfileIDExist(i))
					{
						num++;
					}
				}
				return Mathf.Max(1, num);
			}
			return 1;
		}

		public static void DeleteProfilePrefs(int profileID)
		{
			bool flag = profileID == GetActiveProfileID();
			OptionsFileHandler.DeleteOptions(profileID);
			if (!flag)
			{
				return;
			}
			for (int i = 0; i < maxProfiles; i++)
			{
				if (DoesProfileIDExist(i))
				{
					SwitchProfileID(i);
					return;
				}
			}
			SwitchProfileID(0);
		}

		public bool DoesProfileExist(int index, bool includeActive = true)
		{
			if (index < 0)
			{
				return false;
			}
			int profileID = KickStarter.options.ProfileIndexToID(index, includeActive);
			return DoesProfileIDExist(profileID);
		}

		public static bool DoesProfileIDExist(int profileID)
		{
			if (KickStarter.settingsManager != null && !KickStarter.settingsManager.useProfiles)
			{
				profileID = 0;
			}
			return OptionsFileHandler.DoesProfileExist(profileID);
		}

		public bool DoesProfileExist(string label)
		{
			if (string.IsNullOrEmpty(label))
			{
				return false;
			}
			if (KickStarter.settingsManager != null && !KickStarter.settingsManager.useProfiles)
			{
				return false;
			}
			for (int i = 0; i < maxProfiles; i++)
			{
				string profileName = GetProfileName(i);
				if (profileName == label)
				{
					return true;
				}
			}
			return false;
		}

		public static bool SwitchProfileID(int profileID)
		{
			if (!DoesProfileIDExist(profileID))
			{
				ACDebug.LogWarning("Cannot switch to profile ID " + profileID + ", as it has not been created.");
				return false;
			}
			SetActiveProfileID(profileID);
			LoadPrefs();
			ACDebug.Log("Switched to profile " + profileID + ": '" + optionsData.label + "'");
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles();
				KickStarter.playerMenus.RecalculateAll();
				KickStarter.runtimeVariables.AssignOptionsLinkedVariabes();
			}
			KickStarter.eventManager.Call_OnSwitchProfile(profileID);
			return true;
		}

		public static void UpdateSaveLabels(SaveFile[] foundSaveFiles)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (foundSaveFiles != null)
			{
				foreach (SaveFile saveFile in foundSaveFiles)
				{
					stringBuilder.Append(saveFile.saveID.ToString());
					stringBuilder.Append(":");
					stringBuilder.Append(saveFile.GetSafeLabel());
					stringBuilder.Append("|");
				}
				if (foundSaveFiles.Length > 0)
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
			}
			optionsData.saveFileNames = stringBuilder.ToString();
			SavePrefs();
		}

		public void AfterLoad()
		{
			if (!KickStarter.settingsManager.IsInLoadingScene())
			{
				StartCoroutine(UpdateMixerVolumes());
				SetVolume(SoundType.Music);
				SetVolume(SoundType.SFX);
				SetVolume(SoundType.Speech);
			}
		}

		protected IEnumerator UpdateMixerVolumes()
		{
			yield return null;
			if (KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				if (optionsData == null)
				{
					LoadPrefs();
				}
				AdvGame.SetMixerVolume(KickStarter.settingsManager.musicMixerGroup, KickStarter.settingsManager.musicAttentuationParameter, optionsData.musicVolume);
				AdvGame.SetMixerVolume(KickStarter.settingsManager.sfxMixerGroup, KickStarter.settingsManager.sfxAttentuationParameter, optionsData.sfxVolume);
				AdvGame.SetMixerVolume(KickStarter.settingsManager.speechMixerGroup, KickStarter.settingsManager.speechAttentuationParameter, optionsData.speechVolume);
			}
		}

		public void SetVolume(SoundType _soundType, float newVolume = -1f)
		{
			if (newVolume >= 0f)
			{
				if (optionsData != null)
				{
					switch (_soundType)
					{
					case SoundType.Music:
						optionsData.musicVolume = newVolume;
						break;
					case SoundType.SFX:
						optionsData.sfxVolume = newVolume;
						break;
					case SoundType.Speech:
						optionsData.speechVolume = newVolume;
						break;
					}
					SavePrefs();
					KickStarter.eventManager.Call_OnChangeVolume(_soundType, newVolume);
				}
				else
				{
					ACDebug.LogWarning("Could not find Options data!");
				}
			}
			Sound[] array = UnityEngine.Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			Sound[] array2 = array;
			foreach (Sound sound in array2)
			{
				if (sound.soundType == _soundType)
				{
					sound.AfterLoading();
				}
			}
			KickStarter.dialog.UpdateSpeechVolumes();
		}

		public static void SetLanguage(int i)
		{
			if (optionsData != null)
			{
				optionsData.language = i;
				SavePrefs();
				KickStarter.eventManager.Call_OnChangeLanguage(i);
				KickStarter.runtimeLanguages.LoadAssetBundle(GetVoiceLanguage());
			}
			else
			{
				ACDebug.LogWarning("Could not find Options data!");
			}
		}

		public static void SetVoiceLanguage(int i)
		{
			if (KickStarter.speechManager == null || !KickStarter.speechManager.separateVoiceAndTextLanguages)
			{
				SetLanguage(i);
			}
			else if (optionsData != null)
			{
				optionsData.voiceLanguage = i;
				SavePrefs();
				KickStarter.eventManager.Call_OnChangeVoiceLanguage(i);
				KickStarter.runtimeLanguages.LoadAssetBundle(i);
			}
			else
			{
				ACDebug.LogWarning("Could not find Options data!");
			}
		}

		public static void SetSubtitles(bool showSubtitles)
		{
			if (optionsData != null)
			{
				optionsData.showSubtitles = showSubtitles;
				SavePrefs();
				KickStarter.eventManager.Call_OnChangeSubtitles(showSubtitles);
			}
			else
			{
				ACDebug.LogWarning("Could not find Options data!");
			}
		}

		public static void SetSFXVolume(float newVolume)
		{
			KickStarter.options.SetVolume(SoundType.SFX, newVolume);
			AdvGame.SetMixerVolume(KickStarter.settingsManager.sfxMixerGroup, KickStarter.settingsManager.sfxAttentuationParameter, newVolume);
		}

		public static void SetSpeechVolume(float newVolume)
		{
			KickStarter.options.SetVolume(SoundType.Speech, newVolume);
			AdvGame.SetMixerVolume(KickStarter.settingsManager.speechMixerGroup, KickStarter.settingsManager.speechAttentuationParameter, newVolume);
		}

		public static void SetMusicVolume(float newVolume)
		{
			KickStarter.options.SetVolume(SoundType.Music, newVolume);
			AdvGame.SetMixerVolume(KickStarter.settingsManager.musicMixerGroup, KickStarter.settingsManager.musicAttentuationParameter, newVolume);
		}

		public static string GetLanguageName()
		{
			return KickStarter.runtimeLanguages.Languages[GetLanguage()];
		}

		public static string GetVoiceLanguageName()
		{
			return KickStarter.runtimeLanguages.Languages[GetVoiceLanguage()];
		}

		public static int GetLanguage()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.language;
			}
			return 0;
		}

		public static int GetVoiceLanguage()
		{
			if (Application.isPlaying && optionsData != null)
			{
				if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
				{
					return optionsData.voiceLanguage;
				}
				return optionsData.language;
			}
			return 0;
		}

		public static bool AreSubtitlesOn()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.showSubtitles;
			}
			return false;
		}

		public static float GetSFXVolume()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.sfxVolume;
			}
			return 1f;
		}

		public static float GetMusicVolume()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.musicVolume;
			}
			return 1f;
		}

		public static float GetSpeechVolume()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.speechVolume;
			}
			return 1f;
		}

		protected void CustomSaveOptionsHook()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks(GetComponents(typeof(ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				ISaveOptions[] array = saveOptionsHooks;
				foreach (ISaveOptions saveOptions in array)
				{
					saveOptions.PreSaveOptions();
				}
			}
		}

		protected void CustomLoadOptionsHook()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks(GetComponents(typeof(ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				ISaveOptions[] array = saveOptionsHooks;
				foreach (ISaveOptions saveOptions in array)
				{
					saveOptions.PostLoadOptions();
				}
			}
		}

		protected ISaveOptions[] GetSaveOptionsHooks(IList list)
		{
			ISaveOptions[] array = new ISaveOptions[list.Count];
			list.CopyTo(array, 0);
			return array;
		}
	}
}
