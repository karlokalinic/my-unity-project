using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_languages.html")]
	public class RuntimeLanguages : MonoBehaviour
	{
		protected Dictionary<int, SpeechLine> speechLinesDictionary = new Dictionary<int, SpeechLine>();

		protected List<string> languages = new List<string>();

		protected List<bool> languageIsRightToLeft = new List<bool>();

		protected List<string> languageAudioAssetBundles = new List<string>();

		protected List<string> languageLipsyncAssetBundles = new List<string>();

		protected AssetBundle currentAudioAssetBundle;

		protected string currentAudioAssetBundleName;

		protected AssetBundle currentLipsyncAssetBundle;

		protected string currentLipsyncAssetBundleName;

		protected bool isLoadingBundle;

		protected List<int> spokenOnceSpeechLineIDs = new List<int>();

		public AssetBundle CurrentAudioAssetBundle
		{
			get
			{
				return currentAudioAssetBundle;
			}
			set
			{
				if (currentAudioAssetBundle != value && currentAudioAssetBundle != null)
				{
					currentAudioAssetBundle.Unload(true);
					currentAudioAssetBundle = null;
				}
				currentAudioAssetBundle = value;
			}
		}

		public AssetBundle CurrentLipsyncAssetBundle
		{
			get
			{
				return currentLipsyncAssetBundle;
			}
			set
			{
				if (currentLipsyncAssetBundle != value && currentLipsyncAssetBundle != null)
				{
					currentLipsyncAssetBundle.Unload(true);
					currentLipsyncAssetBundle = null;
				}
				currentLipsyncAssetBundle = value;
			}
		}

		public List<string> Languages
		{
			get
			{
				return languages;
			}
		}

		public bool IsLoadingBundle
		{
			get
			{
				return isLoadingBundle;
			}
		}

		public void OnAwake()
		{
			TransferFromManager();
		}

		public virtual void LoadAssetBundle(int language)
		{
			if (!KickStarter.speechManager.autoNameSpeechFiles || speechLinesDictionary == null || speechLinesDictionary.Count == 0)
			{
				speechLinesDictionary.Clear();
				foreach (SpeechLine line in KickStarter.speechManager.lines)
				{
					if (KickStarter.speechManager.IsTextTypeTranslatable(line.textType))
					{
						speechLinesDictionary.Add(line.lineID, new SpeechLine(line, language));
					}
				}
			}
			if (KickStarter.speechManager.useAssetBundles)
			{
				StopAllCoroutines();
				StartCoroutine(LoadAssetBundleCoroutine(language));
			}
		}

		public virtual AudioClip GetSpeechAudioClip(int lineID, Char _speaker)
		{
			if (!KickStarter.speechManager.IsTextTypeTranslatable(AC_TextType.Speech))
			{
				return null;
			}
			AudioClip audioClip = null;
			int voiceLanguage = Options.GetVoiceLanguage();
			string language = ((voiceLanguage <= 0) ? string.Empty : Options.GetVoiceLanguageName());
			if (KickStarter.speechManager.autoNameSpeechFiles)
			{
				string text = KickStarter.speechManager.GetAutoAssetPathAndName(lineID, _speaker, language);
				if (currentAudioAssetBundle != null)
				{
					if (isLoadingBundle)
					{
						ACDebug.LogWarning("Cannot load audio file from AssetBundle as the AssetBundle is still being loaded.");
						return null;
					}
					int num = text.LastIndexOf("/") + 1;
					if (num > 0)
					{
						text = text.Substring(num);
					}
					audioClip = currentAudioAssetBundle.LoadAsset<AudioClip>(text);
					if (audioClip == null && !string.IsNullOrEmpty(text))
					{
						ACDebug.LogWarning("Audio file '" + text + "' not found in Asset Bundle '" + currentAudioAssetBundle.name + "'.");
					}
				}
				else
				{
					audioClip = Resources.Load(text) as AudioClip;
					if (audioClip == null && KickStarter.speechManager.fallbackAudio && voiceLanguage > 0)
					{
						text = KickStarter.speechManager.GetAutoAssetPathAndName(lineID, _speaker, string.Empty);
						audioClip = Resources.Load(text) as AudioClip;
					}
					if (audioClip == null && !string.IsNullOrEmpty(text))
					{
						ACDebug.LogWarning("Audio file 'Resources/" + text + "' not found in Resources folder.");
					}
				}
			}
			else
			{
				audioClip = GetLineCustomAudioClip(lineID, voiceLanguage);
				if (audioClip == null && KickStarter.speechManager.fallbackAudio && voiceLanguage > 0)
				{
					audioClip = GetLineCustomAudioClip(lineID);
				}
			}
			return audioClip;
		}

		public virtual T GetSpeechLipsyncFile<T>(int lineID, Char _speaker) where T : Object
		{
			if (!KickStarter.speechManager.IsTextTypeTranslatable(AC_TextType.Speech))
			{
				return (T)null;
			}
			T val = (T)null;
			int voiceLanguage = Options.GetVoiceLanguage();
			string language = ((voiceLanguage <= 0) ? string.Empty : Options.GetVoiceLanguageName());
			if (KickStarter.speechManager.autoNameSpeechFiles)
			{
				string text = KickStarter.speechManager.GetAutoAssetPathAndName(lineID, _speaker, language, true);
				if (currentLipsyncAssetBundle != null)
				{
					if (isLoadingBundle)
					{
						ACDebug.LogWarning("Cannot load lipsync file from AssetBundle as the AssetBundle is still being loaded.");
						return (T)null;
					}
					int num = text.LastIndexOf("/") + 1;
					if (num > 0)
					{
						text = text.Substring(num);
					}
					val = currentLipsyncAssetBundle.LoadAsset<T>(text);
					if (val == null && !string.IsNullOrEmpty(text))
					{
						ACDebug.LogWarning(string.Concat("Lipsync file '", text, "' (", typeof(T), ") not found in Asset Bundle '", currentLipsyncAssetBundle.name, "'."));
					}
				}
				else
				{
					val = Resources.Load(text) as T;
					if (val == null && KickStarter.speechManager.fallbackAudio && voiceLanguage > 0)
					{
						text = KickStarter.speechManager.GetAutoAssetPathAndName(lineID, _speaker, string.Empty, true);
						val = Resources.Load(text) as T;
					}
					if (val == null)
					{
						ACDebug.LogWarning(string.Concat("Lipsync file 'Resources/", text, "' (", typeof(T), ") not found in Resources folder."));
					}
				}
			}
			else
			{
				Object lineCustomLipsyncFile = KickStarter.runtimeLanguages.GetLineCustomLipsyncFile(lineID, voiceLanguage);
				if (lineCustomLipsyncFile == null && KickStarter.speechManager.fallbackAudio && voiceLanguage > 0)
				{
					lineCustomLipsyncFile = KickStarter.runtimeLanguages.GetLineCustomLipsyncFile(lineID);
				}
				if (lineCustomLipsyncFile is T)
				{
					val = (T)KickStarter.runtimeLanguages.GetLineCustomLipsyncFile(lineID, voiceLanguage);
				}
			}
			return val;
		}

		public string GetTranslation(string originalText, int _lineID, int language)
		{
			if (language == 0 || string.IsNullOrEmpty(originalText))
			{
				return originalText;
			}
			if (_lineID == -1 || language <= 0)
			{
				ACDebug.Log("Cannot find translation for '" + originalText + "' because the text has not been added to the Speech Manager.");
				return originalText;
			}
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(_lineID, out value))
			{
				if (value.translationText.Count > language - 1)
				{
					return value.translationText[language - 1];
				}
				ACDebug.LogWarning("A translation is being requested that does not exist!");
				return string.Empty;
			}
			if (KickStarter.settingsManager.showDebugLogs != ShowDebugLogs.Never)
			{
				SpeechLine line = KickStarter.speechManager.GetLine(_lineID);
				if (line == null)
				{
					ACDebug.LogWarning("Cannot find translation for '" + originalText + "' because it's Line ID (" + _lineID + ") was not found in the Speech Manager.");
				}
			}
			return originalText;
		}

		public string GetCurrentLanguageText(int _lineID)
		{
			int language = Options.GetLanguage();
			if (_lineID < 0 || language < 0)
			{
				return string.Empty;
			}
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(_lineID, out value))
			{
				if (language == 0)
				{
					return value.text;
				}
				if (value.translationText.Count > language - 1)
				{
					return value.translationText[language - 1];
				}
				ACDebug.LogWarning("A translation is being requested that does not exist!");
			}
			else
			{
				ACDebug.LogWarning("Cannot find translation for line ID " + _lineID + " because it was not found in the Speech Manager.");
			}
			return string.Empty;
		}

		public string[] GetTranslations(int _lineID)
		{
			if (_lineID == -1)
			{
				return null;
			}
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(_lineID, out value))
			{
				return value.translationText.ToArray();
			}
			return null;
		}

		public void UpdateRuntimeTranslation(int lineID, int languageIndex, string translationText)
		{
			if (languageIndex <= 0)
			{
				ACDebug.LogWarning("The language index must be greater than zero.");
			}
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(lineID, out value))
			{
				value.translationText[languageIndex - 1] = translationText;
			}
		}

		public string GetTranslatableText(ITranslatable translatable, int index = 0)
		{
			int language = Options.GetLanguage();
			string translatableString = translatable.GetTranslatableString(index);
			int translationID = translatable.GetTranslationID(index);
			return GetTranslation(translatableString, translationID, language);
		}

		public void ImportRuntimeTranslation(TextAsset textAsset, string languageName, int newTextColumn, bool ignoreEmptyCells = false, bool isRTL = false)
		{
			if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
			{
				if (newTextColumn <= 0)
				{
					ACDebug.LogWarning("Error importing language from " + textAsset.name + " - newTextColumn must be greater than zero, as the first column is reserved for ID numbers.");
				}
				else if (!languages.Contains(languageName))
				{
					CreateLanguage(languageName, isRTL);
					int i = languages.Count - 1;
					ProcessTranslationFile(i, textAsset.text, newTextColumn, ignoreEmptyCells);
					ACDebug.Log("Created new language " + languageName);
				}
				else
				{
					int num = languages.IndexOf(languageName);
					languageIsRightToLeft[num] = isRTL;
					ProcessTranslationFile(num, textAsset.text, newTextColumn, ignoreEmptyCells);
					ACDebug.Log("Updated language " + languageName);
				}
			}
		}

		public bool LanguageReadsRightToLeft(int languageIndex)
		{
			if (languageIsRightToLeft != null && languageIsRightToLeft.Count > languageIndex)
			{
				return languageIsRightToLeft[languageIndex];
			}
			if (languageIsRightToLeft.Count == 0)
			{
				languageIsRightToLeft.Add(false);
			}
			return languageIsRightToLeft[0];
		}

		public bool LanguageReadsRightToLeft(string languageName)
		{
			if (!string.IsNullOrEmpty(languageName) && languages.Contains(languageName))
			{
				int index = languages.IndexOf(languageName);
				return languageIsRightToLeft[index];
			}
			if (languageIsRightToLeft.Count == 0)
			{
				languageIsRightToLeft.Add(false);
			}
			return languageIsRightToLeft[0];
		}

		public bool MarkLineAsSpoken(int lineID)
		{
			if (lineID < 0)
			{
				return true;
			}
			if (spokenOnceSpeechLineIDs.Contains(lineID))
			{
				return false;
			}
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(lineID, out value) && value.onlyPlaySpeechOnce)
			{
				spokenOnceSpeechLineIDs.Add(lineID);
			}
			return true;
		}

		public MainData SaveMainData(MainData mainData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < spokenOnceSpeechLineIDs.Count; i++)
			{
				stringBuilder.Append(spokenOnceSpeechLineIDs[i].ToString());
				stringBuilder.Append(":");
			}
			if (spokenOnceSpeechLineIDs.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			mainData.spokenLinesData = stringBuilder.ToString();
			return mainData;
		}

		public void LoadMainData(MainData mainData)
		{
			spokenOnceSpeechLineIDs.Clear();
			string spokenLinesData = mainData.spokenLinesData;
			if (string.IsNullOrEmpty(spokenLinesData))
			{
				return;
			}
			string[] array = spokenLinesData.Split(":"[0]);
			string[] array2 = array;
			foreach (string s in array2)
			{
				int result = -1;
				if (int.TryParse(s, out result) && result >= 0)
				{
					spokenOnceSpeechLineIDs.Add(result);
				}
			}
		}

		protected void TransferFromManager()
		{
			if (!AdvGame.GetReferences() || !AdvGame.GetReferences().speechManager)
			{
				return;
			}
			SpeechManager speechManager = AdvGame.GetReferences().speechManager;
			languages.Clear();
			foreach (string language in speechManager.languages)
			{
				languages.Add(language);
			}
			languageIsRightToLeft.Clear();
			foreach (bool item in speechManager.languageIsRightToLeft)
			{
				languageIsRightToLeft.Add(item);
			}
			languageAudioAssetBundles.Clear();
			foreach (string languageAudioAssetBundle in speechManager.languageAudioAssetBundles)
			{
				languageAudioAssetBundles.Add(languageAudioAssetBundle);
			}
			languageLipsyncAssetBundles.Clear();
			foreach (string languageLipsyncAssetBundle in speechManager.languageLipsyncAssetBundles)
			{
				languageLipsyncAssetBundles.Add(languageLipsyncAssetBundle);
			}
		}

		protected IEnumerator LoadAssetBundleCoroutine(int i)
		{
			isLoadingBundle = true;
			if (!string.IsNullOrEmpty(languageAudioAssetBundles[i]) && currentAudioAssetBundleName != languageAudioAssetBundles[i] && currentLipsyncAssetBundleName != languageAudioAssetBundles[i])
			{
				string bundlePath = Path.Combine(Application.streamingAssetsPath, languageAudioAssetBundles[i]);
				AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePath);
				yield return bundleLoadRequest;
				CurrentAudioAssetBundle = bundleLoadRequest.assetBundle;
				if (currentAudioAssetBundle == null)
				{
					ACDebug.LogWarning("Failed to load AssetBundle '" + bundlePath + "'");
				}
				else
				{
					currentAudioAssetBundleName = languageAudioAssetBundles[i];
				}
			}
			if (KickStarter.speechManager.UseFileBasedLipSyncing() && !string.IsNullOrEmpty(languageLipsyncAssetBundles[i]) && currentLipsyncAssetBundleName != languageLipsyncAssetBundles[i])
			{
				if (currentAudioAssetBundleName == languageLipsyncAssetBundles[i])
				{
					CurrentLipsyncAssetBundle = currentAudioAssetBundle;
					currentLipsyncAssetBundleName = currentAudioAssetBundleName;
				}
				else
				{
					string bundlePath2 = Path.Combine(Application.streamingAssetsPath, languageLipsyncAssetBundles[i]);
					AssetBundleCreateRequest bundleLoadRequest2 = AssetBundle.LoadFromFileAsync(bundlePath2);
					yield return bundleLoadRequest2;
					CurrentLipsyncAssetBundle = bundleLoadRequest2.assetBundle;
					if (currentLipsyncAssetBundle == null)
					{
						ACDebug.LogWarning("Failed to load AssetBundle '" + bundlePath2 + "'");
					}
					else
					{
						currentLipsyncAssetBundleName = languageLipsyncAssetBundles[i];
					}
				}
			}
			isLoadingBundle = false;
			KickStarter.eventManager.Call_OnLoadSpeechAssetBundle(i);
		}

		protected AudioClip GetLineCustomAudioClip(int _lineID, int _language = 0)
		{
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(_lineID, out value))
			{
				if (!KickStarter.speechManager.translateAudio || _language <= 0)
				{
					return value.customAudioClip;
				}
				if (value.customTranslationAudioClips != null && value.customTranslationAudioClips.Count > _language - 1)
				{
					return value.customTranslationAudioClips[_language - 1];
				}
			}
			return null;
		}

		protected Object GetLineCustomLipsyncFile(int _lineID, int _language = 0)
		{
			SpeechLine value;
			if (speechLinesDictionary.TryGetValue(_lineID, out value))
			{
				if (!KickStarter.speechManager.translateAudio || _language <= 0)
				{
					return value.customLipsyncFile;
				}
				if (value.customTranslationLipsyncFiles != null && value.customTranslationLipsyncFiles.Count > _language - 1)
				{
					return value.customTranslationLipsyncFiles[_language - 1];
				}
			}
			return null;
		}

		protected void CreateLanguage(string name, bool isRTL)
		{
			languages.Add(name);
			languageIsRightToLeft.Add(isRTL);
			foreach (SpeechLine line in KickStarter.speechManager.lines)
			{
				int lineID = line.lineID;
				SpeechLine value = null;
				if (speechLinesDictionary.TryGetValue(lineID, out value))
				{
					value.translationText.Add(value.text);
				}
			}
		}

		protected void ProcessTranslationFile(int i, string csvText, int newTextColumn, bool ignoreEmptyCells)
		{
			string[,] array = CSVReader.SplitCsvGrid(csvText);
			int num = 0;
			string empty = string.Empty;
			if (array.GetLength(0) <= newTextColumn)
			{
				ACDebug.LogWarning("Cannot import translation file, as it does not have enough columns - searching for column index " + newTextColumn);
				return;
			}
			for (int j = 1; j < array.GetLength(1); j++)
			{
				if (array[0, j] == null || array[0, j].Length <= 0)
				{
					continue;
				}
				num = -1;
				if (int.TryParse(array[0, j], out num))
				{
					empty = array[newTextColumn, j];
					empty = AddLineBreaks(empty);
					if (!ignoreEmptyCells || !string.IsNullOrEmpty(empty))
					{
						UpdateRuntimeTranslation(num, i, empty);
					}
				}
				else
				{
					ACDebug.LogWarning("Error importing translation (ID:" + array[0, j] + ") on row #" + j + ".");
				}
			}
		}

		protected string AddLineBreaks(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				return text.Replace("[break]", "\n");
			}
			return string.Empty;
		}
	}
}
