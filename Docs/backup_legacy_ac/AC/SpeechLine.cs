using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class SpeechLine
	{
		public bool isPlayer;

		public int lineID;

		public string scene;

		public string owner;

		public string text;

		public string description;

		public AC_TextType textType;

		public List<string> translationText = new List<string>();

		public AudioClip customAudioClip;

		public UnityEngine.Object customLipsyncFile;

		public List<AudioClip> customTranslationAudioClips;

		public List<UnityEngine.Object> customTranslationLipsyncFiles;

		public int tagID;

		public bool onlyPlaySpeechOnce;

		protected bool gotCommentFromDescription;

		protected static string[] badChars = new string[28]
		{
			"/", "`", "'", "!", "@", "£", "$", "%", "^", "&",
			"*", "(", ")", "{", "}", ":", ";", ".", "|", "<",
			",", ">", "?", "#", "-", "=", "+", "-"
		};

		private static string speechMarkAsString;

		protected static string SpeechMarkAsString
		{
			get
			{
				if (string.IsNullOrEmpty(speechMarkAsString))
				{
					speechMarkAsString = '"'.ToString();
				}
				return speechMarkAsString;
			}
		}

		public SpeechLine(SpeechLine _speechLine)
		{
			isPlayer = _speechLine.isPlayer;
			lineID = _speechLine.lineID;
			scene = _speechLine.scene;
			owner = _speechLine.owner;
			text = _speechLine.text;
			description = _speechLine.description;
			textType = _speechLine.textType;
			translationText = _speechLine.translationText;
			customAudioClip = _speechLine.customAudioClip;
			customLipsyncFile = _speechLine.customLipsyncFile;
			customTranslationAudioClips = _speechLine.customTranslationAudioClips;
			customTranslationLipsyncFiles = _speechLine.customTranslationLipsyncFiles;
			tagID = _speechLine.tagID;
			onlyPlaySpeechOnce = _speechLine.onlyPlaySpeechOnce;
		}

		public SpeechLine(SpeechLine _speechLine, int voiceLanguage)
		{
			isPlayer = _speechLine.isPlayer;
			lineID = _speechLine.lineID;
			scene = _speechLine.scene;
			owner = _speechLine.owner;
			text = _speechLine.text;
			description = _speechLine.description;
			textType = _speechLine.textType;
			translationText = _speechLine.translationText;
			customAudioClip = ((voiceLanguage != 0 && !KickStarter.speechManager.fallbackAudio) ? null : _speechLine.customAudioClip);
			customLipsyncFile = ((voiceLanguage != 0 && !KickStarter.speechManager.fallbackAudio) ? null : _speechLine.customLipsyncFile);
			customTranslationAudioClips = GetAudioListForTranslation(_speechLine.customTranslationAudioClips, voiceLanguage);
			customTranslationLipsyncFiles = GetLipsyncListForTranslation(_speechLine.customTranslationLipsyncFiles, voiceLanguage);
			tagID = _speechLine.tagID;
			onlyPlaySpeechOnce = _speechLine.onlyPlaySpeechOnce;
		}

		public SpeechLine(int _id, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType, bool _isPlayer)
		{
			lineID = _id;
			scene = _scene;
			owner = _owner;
			text = _text;
			textType = _textType;
			description = string.Empty;
			isPlayer = _isPlayer;
			customAudioClip = null;
			customLipsyncFile = null;
			customTranslationAudioClips = new List<AudioClip>();
			customTranslationLipsyncFiles = new List<UnityEngine.Object>();
			tagID = -1;
			onlyPlaySpeechOnce = false;
			translationText = new List<string>();
			for (int i = 0; i < _languagues; i++)
			{
				translationText.Add(_text);
			}
		}

		public bool IsMatch(SpeechLine newLine, bool ignoreID = false)
		{
			if (text == newLine.text && textType == newLine.textType && owner == newLine.owner && (lineID == newLine.lineID || ignoreID))
			{
				return true;
			}
			return false;
		}

		public void TransferActionComment(string comment, string gameObjectName)
		{
			if (!string.IsNullOrEmpty(comment))
			{
				description = comment;
				gotCommentFromDescription = true;
				return;
			}
			if (!string.IsNullOrEmpty(gameObjectName) && string.IsNullOrEmpty(description))
			{
				description = "From: " + gameObjectName;
			}
			gotCommentFromDescription = false;
		}

		public void RestoreBackup(SpeechLine backupLine)
		{
			translationText = backupLine.translationText;
			customAudioClip = backupLine.customAudioClip;
			customLipsyncFile = backupLine.customLipsyncFile;
			customTranslationAudioClips = backupLine.customTranslationAudioClips;
			customTranslationLipsyncFiles = backupLine.customTranslationLipsyncFiles;
			onlyPlaySpeechOnce = backupLine.onlyPlaySpeechOnce;
			if (!gotCommentFromDescription && !string.IsNullOrEmpty(backupLine.description))
			{
				description = backupLine.description;
			}
		}

		public string GetAutoAssetPathAndName(string language, bool forLipsync = false, string overrideName = "")
		{
			return GetRelativePath(language, forLipsync, overrideName) + GetFilename(overrideName) + lineID;
		}

		public bool SeparatePlayerAudio()
		{
			if (isPlayer && textType == AC_TextType.Speech && owner == "Player" && KickStarter.speechManager.usePlayerRealName && KickStarter.speechManager.separateSharedPlayerAudio && KickStarter.settingsManager != null && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				return true;
			}
			return false;
		}

		public string GetFilename(string overrideName = "")
		{
			string empty = string.Empty;
			if (SeparatePlayerAudio() && !string.IsNullOrEmpty(overrideName))
			{
				empty = overrideName;
			}
			else if (!string.IsNullOrEmpty(owner))
			{
				empty = owner;
				if (isPlayer && textType == AC_TextType.Speech && (KickStarter.speechManager == null || !KickStarter.speechManager.usePlayerRealName))
				{
					empty = "Player";
				}
			}
			else
			{
				empty = "Narrator";
			}
			for (int i = 0; i < badChars.Length; i++)
			{
				empty = empty.Replace(badChars[i], "_");
			}
			return empty.Replace(SpeechMarkAsString, "_");
		}

		protected string GetRelativePath(string language, bool forLipsync = false, string overrideName = "")
		{
			string text = ((!forLipsync) ? KickStarter.speechManager.AutoSpeechFolder : KickStarter.speechManager.AutoLipsyncFolder);
			string filename = GetFilename(overrideName);
			if (!string.IsNullOrEmpty(language) && KickStarter.speechManager.translateAudio)
			{
				text = text + language + "/";
			}
			if (KickStarter.speechManager.placeAudioInSubfolders)
			{
				text = text + filename + "/";
			}
			return text;
		}

		protected string GetSpeakerName()
		{
			if (isPlayer && (AdvGame.GetReferences().speechManager == null || !AdvGame.GetReferences().speechManager.usePlayerRealName))
			{
				return "Player";
			}
			return owner;
		}

		protected void SetCustomArraySizes(int newCount)
		{
			if (customTranslationAudioClips == null)
			{
				customTranslationAudioClips = new List<AudioClip>();
			}
			if (customTranslationLipsyncFiles == null)
			{
				customTranslationLipsyncFiles = new List<UnityEngine.Object>();
			}
			if (newCount < 0)
			{
				newCount = 0;
			}
			if (newCount < customTranslationAudioClips.Count)
			{
				customTranslationAudioClips.RemoveRange(newCount, customTranslationAudioClips.Count - newCount);
			}
			else if (newCount > customTranslationAudioClips.Count)
			{
				if (newCount > customTranslationAudioClips.Capacity)
				{
					customTranslationAudioClips.Capacity = newCount;
				}
				for (int i = customTranslationAudioClips.Count; i < newCount; i++)
				{
					customTranslationAudioClips.Add(null);
				}
			}
			if (newCount < customTranslationLipsyncFiles.Count)
			{
				customTranslationLipsyncFiles.RemoveRange(newCount, customTranslationLipsyncFiles.Count - newCount);
			}
			else if (newCount > customTranslationLipsyncFiles.Count)
			{
				if (newCount > customTranslationLipsyncFiles.Capacity)
				{
					customTranslationLipsyncFiles.Capacity = newCount;
				}
				for (int j = customTranslationLipsyncFiles.Count; j < newCount; j++)
				{
					customTranslationLipsyncFiles.Add(null);
				}
			}
		}

		protected List<AudioClip> GetAudioListForTranslation(List<AudioClip> audioClips, int language)
		{
			List<AudioClip> list = new List<AudioClip>();
			if (language > 0)
			{
				int num = language - 1;
				for (int i = 0; i < audioClips.Count; i++)
				{
					list.Add((num != i) ? null : audioClips[i]);
				}
			}
			return list;
		}

		protected List<UnityEngine.Object> GetLipsyncListForTranslation(List<UnityEngine.Object> lipsyncFiles, int language)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			if (language > 0)
			{
				int num = language - 1;
				for (int i = 0; i < lipsyncFiles.Count; i++)
				{
					list.Add((num != i) ? null : lipsyncFiles[i]);
				}
			}
			return list;
		}
	}
}
