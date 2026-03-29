using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ButtonDialog
	{
		public string label = "(Not set)";

		public int lineID = -1;

		public Texture2D icon;

		public CursorIconBase cursorIcon;

		public bool isOn;

		public bool isLocked;

		public ConversationAction conversationAction;

		public Conversation newConversation;

		public int ID;

		public bool hasBeenChosen;

		public bool linkToInventory;

		public int linkedInventoryID;

		public DialogueOption dialogueOption;

		public ActionListAsset assetFile;

		public GameObject customScriptObject;

		public string customScriptFunction = string.Empty;

		public ButtonDialog(int[] idArray)
		{
			label = string.Empty;
			icon = null;
			cursorIcon = new CursorIconBase();
			isOn = true;
			isLocked = false;
			conversationAction = ConversationAction.ReturnToConversation;
			assetFile = null;
			newConversation = null;
			dialogueOption = null;
			lineID = -1;
			ID = 1;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public ButtonDialog(int _ID, string _label, bool _startEnabled, DialogueOption _dialogueOption, bool _endsConversation)
		{
			label = _label;
			icon = null;
			cursorIcon = new CursorIconBase();
			isOn = _startEnabled;
			isLocked = false;
			conversationAction = (_endsConversation ? ConversationAction.Stop : ConversationAction.ReturnToConversation);
			assetFile = null;
			newConversation = null;
			dialogueOption = _dialogueOption;
			lineID = -1;
			ID = _ID;
		}

		public ButtonDialog(int _ID, string _label, bool _startEnabled, ActionListAsset _actionListAsset, bool _endsConversation)
		{
			label = _label;
			icon = null;
			cursorIcon = new CursorIconBase();
			isOn = _startEnabled;
			isLocked = false;
			conversationAction = (_endsConversation ? ConversationAction.Stop : ConversationAction.ReturnToConversation);
			assetFile = _actionListAsset;
			newConversation = null;
			dialogueOption = null;
			lineID = -1;
			ID = _ID;
		}

		public ButtonDialog(int _ID, string _label, bool _isOn)
		{
			label = _label;
			icon = null;
			cursorIcon = new CursorIconBase();
			isOn = _isOn;
			isLocked = false;
			conversationAction = ConversationAction.Stop;
			assetFile = null;
			newConversation = null;
			dialogueOption = null;
			lineID = -1;
			ID = _ID;
		}

		public bool CanShow()
		{
			if (isOn)
			{
				if (!linkToInventory)
				{
					return true;
				}
				if (linkToInventory && KickStarter.runtimeInventory != null && KickStarter.runtimeInventory.IsCarryingItem(linkedInventoryID))
				{
					return true;
				}
			}
			return false;
		}

		public bool Upgrade()
		{
			if (icon != null)
			{
				if (cursorIcon.texture == null)
				{
					cursorIcon.texture = icon;
					return true;
				}
				icon = null;
			}
			return false;
		}
	}
}
