using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ObjectiveState : ITranslatable
	{
		public int ID;

		[SerializeField]
		protected string label;

		public int labelLineID = -1;

		public string description;

		public int descriptionLineID = -1;

		public ObjectiveStateType stateType;

		public string Label
		{
			get
			{
				if (string.IsNullOrEmpty(label))
				{
					label = "(Untitled)";
				}
				return label;
			}
			set
			{
				label = value;
			}
		}

		public ObjectiveState(int _ID, string _label, ObjectiveStateType _stateType)
		{
			ID = _ID;
			stateType = _stateType;
			label = _label;
			labelLineID = -1;
			description = string.Empty;
			descriptionLineID = -1;
		}

		public ObjectiveState(int[] idArray)
		{
			stateType = ObjectiveStateType.Active;
			label = string.Empty;
			labelLineID = -1;
			description = string.Empty;
			descriptionLineID = -1;
			ID = 0;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public string GetLabel(int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation(label, labelLineID, languageNumber);
		}

		public string GetDescription(int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation(description, descriptionLineID, languageNumber);
		}

		public bool DisplayTypeMatches(ObjectiveDisplayType displayType)
		{
			switch (displayType)
			{
			case ObjectiveDisplayType.All:
				return true;
			case ObjectiveDisplayType.ActiveOnly:
				return stateType == ObjectiveStateType.Active;
			case ObjectiveDisplayType.CompleteOnly:
				return stateType == ObjectiveStateType.Complete;
			case ObjectiveDisplayType.FailedOnly:
				return stateType == ObjectiveStateType.Fail;
			default:
				return false;
			}
		}

		public string GetTranslatableString(int index)
		{
			if (index == 0)
			{
				return label;
			}
			return description;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return labelLineID;
			}
			return descriptionLineID;
		}
	}
}
