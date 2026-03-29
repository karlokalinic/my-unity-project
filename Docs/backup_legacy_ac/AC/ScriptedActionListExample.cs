using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class ScriptedActionListExample : MonoBehaviour, ITranslatable
	{
		[Header("Example 1 fields")]
		[SerializeField]
		private Marker markerToMoveTo;

		[SerializeField]
		private int inventoryItemIDToAdd;

		[SerializeField]
		private string playerSpeechText = "This is a scripted ActionList!";

		[HideInInspector]
		[SerializeField]
		private int playerSpeechTranslationID = -1;

		[Header("Example 2 fields")]
		[SerializeField]
		private int globalBoolVariableID;

		[ContextMenu("Run Example 1")]
		public void RunExampleOne()
		{
			ActionList actionList = CreateActionList();
			if (actionList.AreActionsRunning() || !Application.isPlaying)
			{
				Debug.LogWarning("Cannot run Actions at this time", this);
				return;
			}
			actionList.actions = new List<Action>
			{
				ActionComment.CreateNew("Running Example 1 - move the player, say something, and add an inventory item"),
				ActionCharPathFind.CreateNew(KickStarter.player, markerToMoveTo),
				ActionSpeech.CreateNew(KickStarter.player, playerSpeechText, playerSpeechTranslationID),
				ActionInventorySet.CreateNew_Add(inventoryItemIDToAdd),
				ActionComment.CreateNew("Example complete!")
			};
			actionList.Interact();
		}

		[ContextMenu("Run Example 2")]
		private void RunExampleTwo()
		{
			ActionList actionList = CreateActionList();
			if (actionList.AreActionsRunning() || !Application.isPlaying)
			{
				Debug.LogWarning("Cannot run Actions at this time", this);
				return;
			}
			ActionVarCheck actionVarCheck = ActionVarCheck.CreateNew_Global(globalBoolVariableID);
			ActionComment actionComment = ActionComment.CreateNew("The bool variable is currently True!");
			ActionComment actionComment2 = ActionComment.CreateNew("The bool variable is currently False!");
			actionList.actions = new List<Action> { actionVarCheck, actionComment, actionComment2 };
			actionVarCheck.SetOutputs(new ActionEnd(actionComment), new ActionEnd(actionComment2));
			actionComment.SetOutput(new ActionEnd(true));
			actionComment2.SetOutput(new ActionEnd(true));
			actionList.Interact();
		}

		private ActionList CreateActionList()
		{
			ActionList component = base.gameObject.GetComponent<ActionList>();
			if (component != null)
			{
				return component;
			}
			return base.gameObject.AddComponent<ActionList>();
		}

		public string GetTranslatableString(int index)
		{
			return playerSpeechText;
		}

		public int GetTranslationID(int index)
		{
			return playerSpeechTranslationID;
		}
	}
}
