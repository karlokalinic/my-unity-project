using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class SpeechManager : ScriptableObject
	{
		public delegate string GetAutoAssetPathAndNameDelegate(SpeechLine speechLine, string language, bool forLipSync);

		public bool scrollSubtitles = true;

		public bool scrollNarration;

		public float textScrollSpeed = 50f;

		public AudioClip textScrollCLip;

		public AudioClip narrationTextScrollCLip;

		public bool playScrollAudioEveryCharacter = true;

		public bool displayForever;

		public bool displayNarrationForever;

		public bool playAnimationForever = true;

		public bool canSkipWithMouseClicks = true;

		public float minimumDisplayTime = 1f;

		public float screenTimeFactor = 0.1f;

		public bool allowSpeechSkipping;

		public bool allowGameplaySpeechSkipping;

		public float skipThresholdTime;

		public bool endScrollBeforeSkip;

		public bool scrollingTextFactorsLength;

		public bool syncSubtitlesToAudio;

		public bool useAssetBundles;

		public bool searchAudioFiles = true;

		public bool autoNameSpeechFiles = true;

		public string autoSpeechFolder = "Speech";

		public string autoLipsyncFolder = "Lipsync";

		public bool forceSubtitles = true;

		public bool translateAudio = true;

		public bool separateVoiceAndTextLanguages;

		public bool fallbackAudio;

		public bool keepTextInBuffer;

		public bool relegateBackgroundSpeechAudio;

		public bool usePlayerRealName;

		public bool separateSharedPlayerAudio;

		public bool placeAudioInSubfolders;

		public bool separateLines;

		public float separateLinePause = 1f;

		public bool resetExpressionsEachLine = true;

		public List<SpeechLine> lines = new List<SpeechLine>();

		public List<string> languages = new List<string>();

		public List<bool> languageIsRightToLeft = new List<bool>();

		public List<string> languageAudioAssetBundles = new List<string>();

		public List<string> languageLipsyncAssetBundles = new List<string>();

		public bool ignoreOriginalText;

		public float sfxDucking;

		public float musicDucking;

		public LipSyncMode lipSyncMode;

		public LipSyncOutput lipSyncOutput;

		public List<string> phonemes = new List<string>();

		public float lipSyncSpeed = 1f;

		public GetAutoAssetPathAndNameDelegate GetAutoAssetPathAndNameOverride;

		public AC_TextTypeFlags translatableTextTypes = (AC_TextTypeFlags)(-1);

		public string AutoSpeechFolder
		{
			get
			{
				if (string.IsNullOrEmpty(autoSpeechFolder))
				{
					return string.Empty;
				}
				if (!autoSpeechFolder.EndsWith("/"))
				{
					return autoSpeechFolder + "/";
				}
				return autoSpeechFolder;
			}
		}

		public string AutoLipsyncFolder
		{
			get
			{
				if (string.IsNullOrEmpty(autoLipsyncFolder))
				{
					return string.Empty;
				}
				if (!autoLipsyncFolder.EndsWith("/"))
				{
					return autoLipsyncFolder + "/";
				}
				return autoLipsyncFolder;
			}
		}

		private void SyncLanguageData()
		{
			if (languages.Count < languageIsRightToLeft.Count)
			{
				languageIsRightToLeft.RemoveRange(languages.Count, languageIsRightToLeft.Count - languages.Count);
			}
			else if (languages.Count > languageIsRightToLeft.Count)
			{
				if (languages.Count > languageIsRightToLeft.Capacity)
				{
					languageIsRightToLeft.Capacity = languages.Count;
				}
				for (int i = languageIsRightToLeft.Count; i < languages.Count; i++)
				{
					languageIsRightToLeft.Add(false);
				}
			}
			if (languages.Count < languageAudioAssetBundles.Count)
			{
				languageAudioAssetBundles.RemoveRange(languages.Count, languageAudioAssetBundles.Count - languages.Count);
			}
			else if (languages.Count > languageAudioAssetBundles.Count)
			{
				if (languages.Count > languageAudioAssetBundles.Capacity)
				{
					languageAudioAssetBundles.Capacity = languages.Count;
				}
				for (int j = languageAudioAssetBundles.Count; j < languages.Count; j++)
				{
					languageAudioAssetBundles.Add(string.Empty);
				}
			}
			if (languages.Count < languageLipsyncAssetBundles.Count)
			{
				languageLipsyncAssetBundles.RemoveRange(languages.Count, languageLipsyncAssetBundles.Count - languages.Count);
			}
			else if (languages.Count > languageLipsyncAssetBundles.Count)
			{
				if (languages.Count > languageLipsyncAssetBundles.Capacity)
				{
					languageLipsyncAssetBundles.Capacity = languages.Count;
				}
				for (int k = languageLipsyncAssetBundles.Count; k < languages.Count; k++)
				{
					languageLipsyncAssetBundles.Add(string.Empty);
				}
			}
		}

		public string GetLineFilename(int _lineID, string speakerName = "")
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					return line.GetFilename(speakerName);
				}
			}
			return string.Empty;
		}

		public string GetAutoAssetPathAndName(int lineID, Char speaker, string language, bool forLipsync = false)
		{
			SpeechLine line = GetLine(lineID);
			if (line != null)
			{
				if (GetAutoAssetPathAndNameOverride != null)
				{
					return GetAutoAssetPathAndNameOverride(line, language, forLipsync);
				}
				string overrideName = ((!(speaker != null)) ? string.Empty : speaker.name);
				return line.GetAutoAssetPathAndName(language, forLipsync, overrideName);
			}
			return string.Empty;
		}

		public SpeechLine GetLine(int _lineID)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					return line;
				}
			}
			return null;
		}

		public bool UseFileBasedLipSyncing()
		{
			if (lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadPapagayoFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.RogoLipSync)
			{
				return true;
			}
			return false;
		}

		public bool IsTextTypeTranslatable(AC_TextType textType)
		{
			int num = (int)Mathf.Pow(2f, (float)textType);
			int num2 = (int)translatableTextTypes;
			return (num & num2) != 0;
		}
	}
}
