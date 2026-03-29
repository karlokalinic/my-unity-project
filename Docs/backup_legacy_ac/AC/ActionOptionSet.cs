using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionOptionSet : Action
	{
		protected enum OptionSetMethod
		{
			Language = 0,
			Subtitles = 1,
			SFXVolume = 2,
			SpeechVolume = 3,
			MusicVolume = 4
		}

		[SerializeField]
		protected int indexParameterID = -1;

		[SerializeField]
		protected int index;

		[SerializeField]
		protected int volumeParameterID = -1;

		[SerializeField]
		protected float volume;

		[SerializeField]
		protected OptionSetMethod method;

		[SerializeField]
		protected SplitLanguageType splitLanguageType;

		public ActionOptionSet()
		{
			isDisplayed = true;
			category = ActionCategory.Save;
			title = "Set Option";
			description = "Set an Options variable to a specific value";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			switch (method)
			{
			case OptionSetMethod.Language:
				index = AssignInteger(parameters, indexParameterID, index);
				break;
			case OptionSetMethod.Subtitles:
			{
				BoolValue field = (BoolValue)index;
				field = AssignBoolean(parameters, indexParameterID, field);
				index = (int)field;
				break;
			}
			case OptionSetMethod.SFXVolume:
			case OptionSetMethod.SpeechVolume:
			case OptionSetMethod.MusicVolume:
				volume = AssignFloat(parameters, volumeParameterID, volume);
				volume = Mathf.Clamp01(volume);
				break;
			}
		}

		public override float Run()
		{
			switch (method)
			{
			case OptionSetMethod.Language:
				if (index >= 0 && KickStarter.speechManager != null && index < KickStarter.speechManager.languages.Count)
				{
					if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
					{
						switch (splitLanguageType)
						{
						case SplitLanguageType.TextAndVoice:
							Options.SetLanguage(index);
							Options.SetVoiceLanguage(index);
							break;
						case SplitLanguageType.TextOnly:
							Options.SetLanguage(index);
							break;
						case SplitLanguageType.VoiceOnly:
							Options.SetVoiceLanguage(index);
							break;
						}
					}
					else
					{
						Options.SetLanguage(index);
					}
				}
				else
				{
					LogWarning("Could not set language to index: " + index + " - does this language exist?");
				}
				break;
			case OptionSetMethod.Subtitles:
				Options.SetSubtitles(index == 1);
				break;
			case OptionSetMethod.SpeechVolume:
				Options.SetSpeechVolume(volume);
				break;
			case OptionSetMethod.SFXVolume:
				Options.SetSFXVolume(volume);
				break;
			case OptionSetMethod.MusicVolume:
				Options.SetMusicVolume(volume);
				break;
			}
			return 0f;
		}

		public static ActionOptionSet CreateNew_Language(int languageIndex, SplitLanguageType splitLanguageType = SplitLanguageType.TextAndVoice)
		{
			ActionOptionSet actionOptionSet = ScriptableObject.CreateInstance<ActionOptionSet>();
			actionOptionSet.method = OptionSetMethod.Language;
			actionOptionSet.index = languageIndex;
			actionOptionSet.splitLanguageType = splitLanguageType;
			return actionOptionSet;
		}

		public static ActionOptionSet CreateNew_Subtitles(bool newState)
		{
			ActionOptionSet actionOptionSet = ScriptableObject.CreateInstance<ActionOptionSet>();
			actionOptionSet.method = OptionSetMethod.Subtitles;
			actionOptionSet.index = (newState ? 1 : 0);
			return actionOptionSet;
		}

		public static ActionOptionSet CreateNew_SFXVolume(float newVolume)
		{
			ActionOptionSet actionOptionSet = ScriptableObject.CreateInstance<ActionOptionSet>();
			actionOptionSet.method = OptionSetMethod.SFXVolume;
			actionOptionSet.volume = newVolume;
			return actionOptionSet;
		}

		public static ActionOptionSet CreateNew_MusicVolume(float newVolume)
		{
			ActionOptionSet actionOptionSet = ScriptableObject.CreateInstance<ActionOptionSet>();
			actionOptionSet.method = OptionSetMethod.MusicVolume;
			actionOptionSet.volume = newVolume;
			return actionOptionSet;
		}

		public static ActionOptionSet CreateNew_SpeechVolume(float newVolume)
		{
			ActionOptionSet actionOptionSet = ScriptableObject.CreateInstance<ActionOptionSet>();
			actionOptionSet.method = OptionSetMethod.SpeechVolume;
			actionOptionSet.volume = newVolume;
			return actionOptionSet;
		}
	}
}
