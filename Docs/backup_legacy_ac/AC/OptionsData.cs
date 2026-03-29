using System;

namespace AC
{
	[Serializable]
	public class OptionsData
	{
		public int language;

		public int voiceLanguage;

		public bool showSubtitles;

		public float sfxVolume;

		public float musicVolume;

		public float speechVolume;

		public string linkedVariables = string.Empty;

		public string saveFileNames = string.Empty;

		public int lastSaveID = -1;

		public string label;

		public int ID;

		public OptionsData()
		{
			language = 0;
			voiceLanguage = 0;
			showSubtitles = false;
			sfxVolume = 0.9f;
			musicVolume = 0.6f;
			speechVolume = 1f;
			linkedVariables = string.Empty;
			saveFileNames = string.Empty;
			lastSaveID = -1;
			ID = 0;
			label = "Profile " + (ID + 1);
		}

		public OptionsData(int _ID)
		{
			language = 0;
			voiceLanguage = 0;
			showSubtitles = false;
			sfxVolume = 0.9f;
			musicVolume = 0.6f;
			speechVolume = 1f;
			linkedVariables = string.Empty;
			saveFileNames = string.Empty;
			lastSaveID = -1;
			ID = _ID;
			label = "Profile " + (ID + 1);
		}

		public OptionsData(int _language, int _voiceLanguage, bool _showSubtitles, float _sfxVolume, float _musicVolume, float _speechVolume, int _ID)
		{
			language = _language;
			voiceLanguage = _voiceLanguage;
			showSubtitles = _showSubtitles;
			sfxVolume = _sfxVolume;
			musicVolume = _musicVolume;
			speechVolume = _speechVolume;
			linkedVariables = string.Empty;
			saveFileNames = string.Empty;
			lastSaveID = -1;
			ID = _ID;
			label = "Profile " + (ID + 1);
		}

		public OptionsData(OptionsData _optionsData, int _ID)
		{
			language = _optionsData.language;
			voiceLanguage = _optionsData.voiceLanguage;
			showSubtitles = _optionsData.showSubtitles;
			sfxVolume = _optionsData.sfxVolume;
			musicVolume = _optionsData.musicVolume;
			speechVolume = _optionsData.speechVolume;
			linkedVariables = _optionsData.linkedVariables;
			saveFileNames = _optionsData.saveFileNames;
			lastSaveID = -1;
			ID = _ID;
			label = "Profile " + (ID + 1);
		}
	}
}
