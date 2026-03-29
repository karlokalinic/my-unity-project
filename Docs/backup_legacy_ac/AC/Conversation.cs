using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Logic/Conversation")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_conversation.html")]
	public class Conversation : MonoBehaviour, ITranslatable
	{
		public InteractionSource interactionSource;

		public List<ButtonDialog> options = new List<ButtonDialog>();

		public ButtonDialog selectedOption;

		public int lastOption = -1;

		public bool autoPlay;

		public bool isTimed;

		public float timer = 5f;

		public int defaultOption;

		protected float startTime;

		protected void Awake()
		{
			Upgrade();
		}

		public void Interact()
		{
			Interact(null);
		}

		public void Interact(ActionConversation actionConversation)
		{
			KickStarter.actionListManager.SetConversationPoint(actionConversation);
			KickStarter.eventManager.Call_OnStartConversation(this);
			CancelInvoke("RunDefault");
			int num = 0;
			foreach (ButtonDialog option in options)
			{
				if (option.CanShow())
				{
					num++;
				}
			}
			if ((bool)KickStarter.playerInput)
			{
				if (num == 1 && autoPlay)
				{
					foreach (ButtonDialog option2 in options)
					{
						if (option2.CanShow())
						{
							RunOption(option2);
							return;
						}
					}
				}
				else if (num > 0)
				{
					KickStarter.playerInput.activeConversation = this;
					KickStarter.stateHandler.gameState = GameState.DialogOptions;
				}
				else
				{
					KickStarter.playerInput.EndConversation();
				}
			}
			if (isTimed)
			{
				startTime = Time.time;
				Invoke("RunDefault", timer);
			}
		}

		public void TurnOn()
		{
			Interact();
		}

		public bool IsActive(bool includeResponses)
		{
			if (KickStarter.playerInput.activeConversation == this || KickStarter.playerInput.PendingOptionConversation == this)
			{
				return true;
			}
			if (includeResponses)
			{
				foreach (ButtonDialog option in options)
				{
					if (interactionSource == InteractionSource.InScene)
					{
						if (KickStarter.actionListManager.IsListRunning(option.dialogueOption))
						{
							return true;
						}
					}
					else if (interactionSource == InteractionSource.AssetFile && KickStarter.actionListAssetManager.IsListRunning(option.assetFile))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void TurnOff()
		{
			if (KickStarter.playerInput != null && KickStarter.playerInput.activeConversation == this)
			{
				CancelInvoke("RunDefault");
				KickStarter.playerInput.EndConversation();
				KickStarter.actionListManager.OnEndConversation();
				KickStarter.actionListManager.SetCorrectGameState();
			}
		}

		public void RunOption(int slot, bool force = false)
		{
			CancelInvoke("RunDefault");
			int num = ConvertSlotToOption(slot, force);
			if (num != -1)
			{
				KickStarter.playerInput.EndConversation();
				if (interactionSource == InteractionSource.CustomScript)
				{
					RunOption(options[num]);
				}
				else
				{
					StartCoroutine(RunOptionCo(num));
				}
			}
		}

		public float GetTimeRemaining()
		{
			return (startTime + timer - Time.time) / timer;
		}

		public string GetOptionName(int slot)
		{
			int num = ConvertSlotToOption(slot);
			if (num == -1)
			{
				num = 0;
			}
			string translation = KickStarter.runtimeLanguages.GetTranslation(options[num].label, options[num].lineID, Options.GetLanguage());
			return AdvGame.ConvertTokens(translation);
		}

		public CursorIconBase GetOptionIcon(int slot)
		{
			int num = ConvertSlotToOption(slot);
			if (num == -1)
			{
				num = 0;
			}
			return options[num].cursorIcon;
		}

		public ButtonDialog GetOption(int slot)
		{
			int num = ConvertSlotToOption(slot);
			if (num == -1)
			{
				num = 0;
			}
			return options[num];
		}

		public ButtonDialog GetOptionWithID(int id)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].ID == id)
				{
					return options[i];
				}
			}
			return null;
		}

		public int GetNumEnabledOptions()
		{
			int num = 0;
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].isOn)
				{
					num++;
				}
			}
			return num;
		}

		public bool OptionHasBeenChosen(int slot)
		{
			int num = ConvertSlotToOption(slot);
			if (num == -1)
			{
				num = 0;
			}
			return options[num].hasBeenChosen;
		}

		public void TurnOptionOn(int id)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isOn = true;
					}
					else
					{
						ACDebug.Log(base.gameObject.name + "'s option '" + option.label + "' cannot be turned on as it is locked.", this);
					}
					break;
				}
			}
		}

		public void TurnOptionOff(int id)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isOn = false;
					}
					else
					{
						ACDebug.LogWarning(base.gameObject.name + "'s option '" + option.label + "' cannot be turned off as it is locked.", this);
					}
					break;
				}
			}
		}

		public void SetOptionState(int id, bool flag, bool isLocked)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isLocked = isLocked;
						option.isOn = flag;
					}
					KickStarter.playerMenus.RefreshDialogueOptions();
					break;
				}
			}
		}

		public void TurnAllOptionsOn(bool includingLocked)
		{
			foreach (ButtonDialog option in options)
			{
				if (includingLocked || !option.isLocked)
				{
					option.isLocked = false;
					option.isOn = true;
				}
			}
		}

		public void RenameOption(int id, string newLabel, int newLineID)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					option.label = newLabel;
					option.lineID = newLineID;
					break;
				}
			}
		}

		public int GetCount()
		{
			int num = 0;
			foreach (ButtonDialog option in options)
			{
				if (option.CanShow())
				{
					num++;
				}
			}
			return num;
		}

		public void Upgrade()
		{
			bool flag = false;
			if (options.Count > 0 && options[0].ID == 0)
			{
				for (int i = 0; i < options.Count; i++)
				{
					options[i].ID = i + 1;
				}
				flag = true;
			}
			for (int j = 0; j < options.Count; j++)
			{
				if (options[j].Upgrade())
				{
					flag = true;
				}
			}
			if (!flag)
			{
			}
		}

		public int[] GetIDArray()
		{
			List<int> list = new List<int>();
			foreach (ButtonDialog option in options)
			{
				list.Add(option.ID);
			}
			list.Sort();
			return list.ToArray();
		}

		protected void RunOption(ButtonDialog _option)
		{
			KickStarter.actionListManager.SetCorrectGameState();
			_option.hasBeenChosen = true;
			if (options.Contains(_option))
			{
				lastOption = options.IndexOf(_option);
				if (KickStarter.actionListManager.OverrideConversation(lastOption))
				{
					KickStarter.eventManager.Call_OnClickConversation(this, _option.ID);
					return;
				}
				lastOption = -1;
			}
			Conversation conversation = null;
			if (interactionSource != InteractionSource.CustomScript)
			{
				if (_option.conversationAction == ConversationAction.ReturnToConversation)
				{
					conversation = this;
				}
				else if (_option.conversationAction == ConversationAction.RunOtherConversation && _option.newConversation != null)
				{
					conversation = _option.newConversation;
				}
			}
			if (interactionSource == InteractionSource.AssetFile && (bool)_option.assetFile)
			{
				AdvGame.RunActionListAsset(_option.assetFile, conversation);
			}
			else if (interactionSource == InteractionSource.CustomScript)
			{
				if (_option.customScriptObject != null && !string.IsNullOrEmpty(_option.customScriptFunction))
				{
					_option.customScriptObject.SendMessage(_option.customScriptFunction);
				}
			}
			else if (interactionSource == InteractionSource.InScene && (bool)_option.dialogueOption)
			{
				_option.dialogueOption.conversation = conversation;
				_option.dialogueOption.Interact();
			}
			else
			{
				ACDebug.Log("No DialogueOption object found on Conversation '" + base.gameObject.name + "'", this);
				if (conversation != null)
				{
					conversation.Interact();
				}
				else
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
				}
			}
			KickStarter.eventManager.Call_OnClickConversation(this, _option.ID);
		}

		protected void RunDefault()
		{
			if ((bool)KickStarter.playerInput && KickStarter.playerInput.IsInConversation())
			{
				if (defaultOption < 0 || defaultOption >= options.Count)
				{
					TurnOff();
				}
				else
				{
					RunOption(defaultOption, true);
				}
			}
		}

		protected IEnumerator RunOptionCo(int i)
		{
			KickStarter.playerInput.PendingOptionConversation = this;
			yield return new WaitForSeconds(KickStarter.dialog.conversationDelay);
			RunOption(options[i]);
			if (KickStarter.playerInput.PendingOptionConversation == this)
			{
				KickStarter.playerInput.PendingOptionConversation = null;
			}
		}

		protected int ConvertSlotToOption(int slot, bool force = false)
		{
			int num = 0;
			for (int i = 0; i < options.Count; i++)
			{
				if (force || options[i].CanShow())
				{
					num++;
					if (num == slot + 1)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public string GetTranslatableString(int index)
		{
			return options[index].label;
		}

		public int GetTranslationID(int index)
		{
			return options[index].lineID;
		}
	}
}
