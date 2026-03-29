using System;

namespace AC
{
	[Serializable]
	public struct MainData
	{
		public int currentPlayerID;

		public float timeScale;

		public string runtimeVariablesData;

		public string customTokenData;

		public string menuLockData;

		public string menuVisibilityData;

		public string menuElementVisibilityData;

		public string menuJournalData;

		public int activeArrows;

		public int activeConversation;

		public int selectedInventoryID;

		public bool isGivingItem;

		public bool cursorIsOff;

		public bool inputIsOff;

		public bool interactionIsOff;

		public bool menuIsOff;

		public bool movementIsOff;

		public bool cameraIsOff;

		public bool triggerIsOff;

		public bool playerIsOff;

		public bool canKeyboardControlMenusDuringGameplay;

		public int toggleCursorState;

		public string musicQueueData;

		public string lastMusicQueueData;

		public int musicTimeSamples;

		public int lastMusicTimeSamples;

		public string oldMusicTimeSamples;

		public string ambienceQueueData;

		public string lastAmbienceQueueData;

		public int ambienceTimeSamples;

		public int lastAmbienceTimeSamples;

		public string oldAmbienceTimeSamples;

		public int movementMethod;

		public string activeAssetLists;

		public string activeInputsData;

		public string spokenLinesData;

		public string globalObjectivesData;
	}
}
