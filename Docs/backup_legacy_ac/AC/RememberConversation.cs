using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Conversation")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_conversation.html")]
	public class RememberConversation : Remember
	{
		private Conversation conversation;

		private Conversation _Conversation
		{
			get
			{
				if (conversation == null)
				{
					conversation = GetComponent<Conversation>();
				}
				return conversation;
			}
		}

		public override string SaveData()
		{
			ConversationData conversationData = new ConversationData();
			conversationData.objectID = constantID;
			conversationData.savePrevented = savePrevented;
			if (_Conversation != null)
			{
				List<bool> list = new List<bool>();
				List<bool> list2 = new List<bool>();
				List<bool> list3 = new List<bool>();
				List<string> list4 = new List<string>();
				List<int> list5 = new List<int>();
				foreach (ButtonDialog option in _Conversation.options)
				{
					list.Add(option.isOn);
					list2.Add(option.isLocked);
					list3.Add(option.hasBeenChosen);
					list4.Add(option.label);
					list5.Add(option.lineID);
				}
				conversationData._optionStates = ArrayToString(list.ToArray());
				conversationData._optionLocks = ArrayToString(list2.ToArray());
				conversationData._optionChosens = ArrayToString(list3.ToArray());
				conversationData._optionLabels = ArrayToString(list4.ToArray());
				conversationData._optionLineIDs = ArrayToString(list5.ToArray());
				conversationData.lastOption = _Conversation.lastOption;
			}
			return Serializer.SaveScriptData<ConversationData>(conversationData);
		}

		public override void LoadData(string stringData)
		{
			ConversationData conversationData = Serializer.LoadScriptData<ConversationData>(stringData);
			if (conversationData == null)
			{
				return;
			}
			base.SavePrevented = conversationData.savePrevented;
			if (savePrevented || !(_Conversation != null))
			{
				return;
			}
			bool[] array = StringToBoolArray(conversationData._optionStates);
			bool[] array2 = StringToBoolArray(conversationData._optionLocks);
			bool[] array3 = StringToBoolArray(conversationData._optionChosens);
			string[] array4 = StringToStringArray(conversationData._optionLabels);
			int[] array5 = StringToIntArray(conversationData._optionLineIDs);
			for (int i = 0; i < _Conversation.options.Count; i++)
			{
				if (array != null && array.Length > i)
				{
					_Conversation.options[i].isOn = array[i];
				}
				if (array2 != null && array2.Length > i)
				{
					_Conversation.options[i].isLocked = array2[i];
				}
				if (array3 != null && array3.Length > i)
				{
					_Conversation.options[i].hasBeenChosen = array3[i];
				}
				if (array4 != null && array4.Length > i)
				{
					_Conversation.options[i].label = array4[i];
				}
				if (array5 != null && array5.Length > i)
				{
					_Conversation.options[i].lineID = array5[i];
				}
			}
			_Conversation.lastOption = conversationData.lastOption;
		}
	}
}
