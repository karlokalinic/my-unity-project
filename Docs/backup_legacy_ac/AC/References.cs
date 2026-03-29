using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class References : ScriptableObject
	{
		public ActionsManager actionsManager;

		public SceneManager sceneManager;

		public SettingsManager settingsManager;

		public InventoryManager inventoryManager;

		public VariablesManager variablesManager;

		public SpeechManager speechManager;

		public CursorManager cursorManager;

		public MenuManager menuManager;

		[NonSerialized]
		[HideInInspector]
		public bool viewingMenuManager;
	}
}
