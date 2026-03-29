using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionType
	{
		public string fileName;

		public ActionCategory category;

		public string title;

		public string description;

		public bool isEnabled;

		public Color color;

		public ActionType(string _fileName, Action _action)
		{
			fileName = _fileName;
			category = _action.category;
			title = _action.title;
			description = _action.description;
			isEnabled = true;
			color = Color.white;
		}

		public ActionType(ActionType _actionType)
		{
			fileName = _actionType.fileName;
			category = _actionType.category;
			title = _actionType.title;
			description = _actionType.description;
			isEnabled = _actionType.isEnabled;
			color = _actionType.color;
		}

		public bool IsMatch(ActionType _actionType)
		{
			if (_actionType != null && description == _actionType.description && title == _actionType.title && category == _actionType.category)
			{
				return true;
			}
			return false;
		}

		public string GetFullTitle(bool forSorting = false)
		{
			if (forSorting && category == ActionCategory.Custom)
			{
				return "ZZ" + title;
			}
			return category.ToString() + ": " + title;
		}
	}
}
