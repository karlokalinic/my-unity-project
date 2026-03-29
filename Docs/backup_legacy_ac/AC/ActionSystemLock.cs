using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSystemLock : Action
	{
		public bool changeMovementMethod;

		public MovementMethod newMovementMethod;

		public LockType cursorLock = LockType.NoChange;

		public LockType inputLock = LockType.NoChange;

		public LockType interactionLock = LockType.NoChange;

		public LockType draggableLock = LockType.NoChange;

		public LockType menuLock = LockType.NoChange;

		public LockType movementLock = LockType.NoChange;

		public LockType cameraLock = LockType.NoChange;

		public LockType triggerLock = LockType.NoChange;

		public LockType playerLock = LockType.NoChange;

		public LockType saveLock = LockType.NoChange;

		public LockType keyboardGameplayMenusLock = LockType.NoChange;

		public ActionSystemLock()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Manage systems";
			description = "Enables and disables individual systems within Adventure Creator, such as Interactions. Can also be used to change the 'Movement method', as set in the Settings Manager, but note that this change will not be recorded in save games.";
		}

		public override float Run()
		{
			if (changeMovementMethod)
			{
				KickStarter.playerInput.InitialiseCursorLock(newMovementMethod);
				KickStarter.settingsManager.movementMethod = newMovementMethod;
			}
			if (cursorLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetCursorSystem(true);
			}
			else if (cursorLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetCursorSystem(false);
			}
			if (inputLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetInputSystem(true);
			}
			else if (inputLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetInputSystem(false);
			}
			if (interactionLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetInteractionSystem(true);
			}
			else if (interactionLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetInteractionSystem(false);
			}
			if (draggableLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetDraggableSystem(true);
			}
			else if (draggableLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetDraggableSystem(false);
			}
			if (menuLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetMenuSystem(true);
			}
			else if (menuLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetMenuSystem(false);
			}
			if (movementLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetMovementSystem(true);
			}
			else if (movementLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetMovementSystem(false);
			}
			if (cameraLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetCameraSystem(true);
			}
			else if (cameraLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetCameraSystem(false);
			}
			if (triggerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetTriggerSystem(true);
			}
			else if (triggerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetTriggerSystem(false);
			}
			if (playerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetPlayerSystem(true);
			}
			else if (playerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetPlayerSystem(false);
			}
			if (saveLock == LockType.Disabled)
			{
				KickStarter.playerMenus.SetManualSaveLock(true);
			}
			else if (saveLock == LockType.Enabled)
			{
				KickStarter.playerMenus.SetManualSaveLock(false);
			}
			if (AdvGame.GetReferences() != null && AdvGame.GetReferences().settingsManager != null && AdvGame.GetReferences().settingsManager.inputMethod != InputMethod.TouchScreen)
			{
				if (keyboardGameplayMenusLock == LockType.Disabled)
				{
					KickStarter.playerInput.canKeyboardControlMenusDuringGameplay = false;
				}
				else if (keyboardGameplayMenusLock == LockType.Enabled)
				{
					KickStarter.playerInput.canKeyboardControlMenusDuringGameplay = true;
				}
			}
			return 0f;
		}

		public static ActionSystemLock CreateNew(LockType cursorLock = LockType.NoChange, LockType inputLock = LockType.NoChange, LockType interactionLock = LockType.NoChange, LockType menuLock = LockType.NoChange, LockType movementLock = LockType.NoChange, LockType cameraLock = LockType.NoChange, LockType triggerLock = LockType.NoChange, LockType playerLock = LockType.NoChange, LockType saveLock = LockType.NoChange, LockType directControlInGameMenusLock = LockType.NoChange, LockType draggableLock = LockType.NoChange)
		{
			ActionSystemLock actionSystemLock = ScriptableObject.CreateInstance<ActionSystemLock>();
			actionSystemLock.changeMovementMethod = false;
			actionSystemLock.cursorLock = cursorLock;
			actionSystemLock.inputLock = inputLock;
			actionSystemLock.interactionLock = interactionLock;
			actionSystemLock.draggableLock = draggableLock;
			actionSystemLock.menuLock = menuLock;
			actionSystemLock.movementLock = movementLock;
			actionSystemLock.cameraLock = cameraLock;
			actionSystemLock.triggerLock = triggerLock;
			actionSystemLock.playerLock = playerLock;
			actionSystemLock.saveLock = saveLock;
			actionSystemLock.keyboardGameplayMenusLock = directControlInGameMenusLock;
			return actionSystemLock;
		}

		public static ActionSystemLock CreateNew(MovementMethod newMovementMethod, LockType cursorLock = LockType.NoChange, LockType inputLock = LockType.NoChange, LockType interactionLock = LockType.NoChange, LockType menuLock = LockType.NoChange, LockType movementLock = LockType.NoChange, LockType cameraLock = LockType.NoChange, LockType triggerLock = LockType.NoChange, LockType playerLock = LockType.NoChange, LockType saveLock = LockType.NoChange, LockType directControlInGameMenusLock = LockType.NoChange, LockType draggableLock = LockType.NoChange)
		{
			ActionSystemLock actionSystemLock = ScriptableObject.CreateInstance<ActionSystemLock>();
			actionSystemLock.changeMovementMethod = true;
			actionSystemLock.newMovementMethod = newMovementMethod;
			actionSystemLock.cursorLock = cursorLock;
			actionSystemLock.inputLock = inputLock;
			actionSystemLock.interactionLock = interactionLock;
			actionSystemLock.draggableLock = draggableLock;
			actionSystemLock.menuLock = menuLock;
			actionSystemLock.movementLock = movementLock;
			actionSystemLock.cameraLock = cameraLock;
			actionSystemLock.triggerLock = triggerLock;
			actionSystemLock.playerLock = playerLock;
			actionSystemLock.saveLock = saveLock;
			actionSystemLock.keyboardGameplayMenusLock = directControlInGameMenusLock;
			return actionSystemLock;
		}
	}
}
