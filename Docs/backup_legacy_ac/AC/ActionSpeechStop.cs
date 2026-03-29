using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSpeechStop : Action
	{
		public bool forceMenus;

		public SpeechMenuLimit speechMenuLimit;

		public SpeechMenuType speechMenuType;

		public string limitToCharacters = string.Empty;

		public ActionSpeechStop()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Stop speech";
			description = "Ends any currently-playing speech instantly.";
		}

		public override float Run()
		{
			KickStarter.dialog.KillDialog(true, forceMenus, speechMenuLimit, speechMenuType, limitToCharacters);
			return 0f;
		}

		public static ActionSpeechStop CreateNew(SpeechMenuLimit speechToStop, SpeechMenuType charactersToStop, string specificCharacters = "", bool forceOffSubtitles = false)
		{
			ActionSpeechStop actionSpeechStop = ScriptableObject.CreateInstance<ActionSpeechStop>();
			actionSpeechStop.speechMenuLimit = speechToStop;
			actionSpeechStop.speechMenuType = charactersToStop;
			actionSpeechStop.limitToCharacters = specificCharacters;
			actionSpeechStop.forceMenus = forceOffSubtitles;
			return actionSpeechStop;
		}
	}
}
