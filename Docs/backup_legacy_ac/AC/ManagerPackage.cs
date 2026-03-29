using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ManagerPackage : ScriptableObject
	{
		public ActionsManager actionsManager;

		public SceneManager sceneManager;

		public SettingsManager settingsManager;

		public InventoryManager inventoryManager;

		public VariablesManager variablesManager;

		public SpeechManager speechManager;

		public CursorManager cursorManager;

		public MenuManager menuManager;

		public bool IsFullyAssigned()
		{
			if (actionsManager == null)
			{
				return false;
			}
			if (sceneManager == null)
			{
				return false;
			}
			if (settingsManager == null)
			{
				return false;
			}
			if (inventoryManager == null)
			{
				return false;
			}
			if (variablesManager == null)
			{
				return false;
			}
			if (speechManager == null)
			{
				return false;
			}
			if (cursorManager == null)
			{
				return false;
			}
			if (menuManager == null)
			{
				return false;
			}
			return true;
		}

		public void AssignManagers()
		{
			if (AdvGame.GetReferences() != null)
			{
				int num = 0;
				if ((bool)sceneManager)
				{
					AdvGame.GetReferences().sceneManager = sceneManager;
					num++;
				}
				if ((bool)settingsManager)
				{
					AdvGame.GetReferences().settingsManager = settingsManager;
					num++;
				}
				if ((bool)actionsManager)
				{
					AdvGame.GetReferences().actionsManager = actionsManager;
					num++;
				}
				if ((bool)variablesManager)
				{
					AdvGame.GetReferences().variablesManager = variablesManager;
					num++;
				}
				if ((bool)inventoryManager)
				{
					AdvGame.GetReferences().inventoryManager = inventoryManager;
					num++;
				}
				if ((bool)speechManager)
				{
					AdvGame.GetReferences().speechManager = speechManager;
					num++;
				}
				if ((bool)cursorManager)
				{
					AdvGame.GetReferences().cursorManager = cursorManager;
					num++;
				}
				if ((bool)menuManager)
				{
					AdvGame.GetReferences().menuManager = menuManager;
					num++;
				}
				if ((bool)this)
				{
					switch (num)
					{
					case 0:
						ACDebug.Log(base.name + " No Mangers assigned.");
						break;
					case 1:
						ACDebug.Log(base.name + " - (" + num + ") Manager assigned.", this);
						break;
					default:
						ACDebug.Log(base.name + " - (" + num + ") Managers assigned.", this);
						break;
					}
				}
			}
			else
			{
				ACDebug.LogError("Can't assign managers - no References file found in Resources folder.");
			}
		}
	}
}
