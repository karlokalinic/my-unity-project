using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionsManager : ScriptableObject
	{
		public bool displayActionsInInspector = true;

		public DisplayActionsInEditor displayActionsInEditor = DisplayActionsInEditor.ArrangedVertically;

		public bool allowMultipleActionListWindows;

		public ActionListEditorScrollWheel actionListEditorScrollWheel;

		public bool invertPanning;

		public float panSpeed = 1f;

		public bool autoPanNearWindowEdge = true;

		public int defaultClass;

		public string defaultClassName;

		public List<ActionType> AllActions = new List<ActionType>();

		public static string GetDefaultAction()
		{
			if (AdvGame.GetReferences() != null && AdvGame.GetReferences().actionsManager != null)
			{
				return AdvGame.GetReferences().actionsManager._GetDefaultAction();
			}
			ACDebug.LogError("Cannot create Action - no Actions Manager found.");
			return string.Empty;
		}

		private string _GetDefaultAction()
		{
			Upgrade();
			if (!string.IsNullOrEmpty(defaultClassName))
			{
				return defaultClassName;
			}
			ACDebug.LogError("Cannot create default Action - no default set.");
			return string.Empty;
		}

		private void Upgrade()
		{
			if (defaultClass >= 0 && AllActions.Count > 0 && AllActions.Count > defaultClass)
			{
				defaultClassName = AllActions[defaultClass].fileName;
				defaultClass = -1;
			}
			if (string.IsNullOrEmpty(defaultClassName) && AllActions.Count > 0)
			{
				defaultClassName = AllActions[0].fileName;
				defaultClass = -1;
			}
		}

		public string GetActionTypeLabel(Action _action)
		{
			int actionTypeIndex = GetActionTypeIndex(_action);
			if (actionTypeIndex >= 0 && AllActions != null && actionTypeIndex < AllActions.Count)
			{
				return AllActions[actionTypeIndex].GetFullTitle();
			}
			return string.Concat(_action.category, ": ", _action.title);
		}

		public string GetActionName(int i)
		{
			return AllActions[i].fileName;
		}

		public bool DoesActionExist(string _name)
		{
			foreach (ActionType allAction in AllActions)
			{
				if (_name == allAction.fileName || _name == "AC." + allAction.fileName)
				{
					return true;
				}
			}
			return false;
		}

		public int GetActionsSize()
		{
			return AllActions.Count;
		}

		public string[] GetActionTitles()
		{
			List<string> list = new List<string>();
			foreach (ActionType allAction in AllActions)
			{
				list.Add(allAction.title);
			}
			return list.ToArray();
		}

		public int GetActionTypeIndex(Action _action)
		{
			string text = _action.GetType().ToString();
			text = text.Replace("AC.", string.Empty);
			foreach (ActionType allAction in AllActions)
			{
				if (allAction.fileName == text)
				{
					return AllActions.IndexOf(allAction);
				}
			}
			return defaultClass;
		}

		public int GetEnabledActionTypeIndex(ActionCategory _category, int subCategoryIndex)
		{
			List<ActionType> list = new List<ActionType>();
			foreach (ActionType allAction in AllActions)
			{
				if (allAction.category == _category && allAction.isEnabled)
				{
					list.Add(allAction);
				}
			}
			if (subCategoryIndex < list.Count)
			{
				return AllActions.IndexOf(list[subCategoryIndex]);
			}
			return 0;
		}

		public string[] GetActionSubCategories(ActionCategory _category)
		{
			List<string> list = new List<string>();
			foreach (ActionType allAction in AllActions)
			{
				if (allAction.category == _category && allAction.isEnabled)
				{
					list.Add(allAction.title);
				}
			}
			return list.ToArray();
		}

		public ActionCategory GetActionCategory(int number)
		{
			if (AllActions == null || AllActions.Count == 0 || AllActions.Count < number)
			{
				return ActionCategory.ActionList;
			}
			return AllActions[number].category;
		}

		public int GetActionSubCategory(Action _action)
		{
			string text = _action.GetType().ToString().Replace("AC.", string.Empty);
			ActionCategory category = _action.category;
			foreach (ActionType allAction in AllActions)
			{
				if (allAction.fileName == text)
				{
					category = allAction.category;
				}
			}
			int num = 0;
			foreach (ActionType allAction2 in AllActions)
			{
				if (allAction2.category == category)
				{
					if (allAction2.fileName == text)
					{
						return num;
					}
					num++;
				}
			}
			ACDebug.LogWarning("Error building Action " + _action);
			return 0;
		}

		public ActionType[] GetActionTypesInCategory(ActionCategory category)
		{
			List<ActionType> list = new List<ActionType>();
			foreach (ActionType allAction in AllActions)
			{
				if (allAction.category == category)
				{
					list.Add(allAction);
				}
			}
			return list.ToArray();
		}

		public bool IsActionTypeEnabled(int index)
		{
			if (AllActions != null && index < AllActions.Count)
			{
				return AllActions[index].isEnabled;
			}
			return false;
		}

		public Color GetActionTypeColor(Action _action)
		{
			int actionTypeIndex = GetActionTypeIndex(_action);
			if (actionTypeIndex >= 0 && AllActions != null && actionTypeIndex < AllActions.Count)
			{
				return GUI.color = AllActions[actionTypeIndex].color;
			}
			return Color.white;
		}
	}
}
