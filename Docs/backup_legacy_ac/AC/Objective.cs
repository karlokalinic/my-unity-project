using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class Objective : ITranslatable
	{
		public int ID;

		[SerializeField]
		protected string title;

		public int titleLineID = -1;

		public string description;

		public int descriptionLineID = -1;

		public bool perPlayer;

		public Texture2D texture;

		public List<ObjectiveState> states;

		public bool lockStateWhenComplete = true;

		public bool lockStateWhenFail = true;

		public string Title
		{
			get
			{
				if (string.IsNullOrEmpty(title))
				{
					title = "(Untitled)";
				}
				return title;
			}
			set
			{
				title = value;
			}
		}

		public int NumStates
		{
			get
			{
				return states.Count;
			}
		}

		public Objective(int[] idArray)
		{
			title = string.Empty;
			titleLineID = -1;
			description = string.Empty;
			descriptionLineID = -1;
			perPlayer = false;
			texture = null;
			states = new List<ObjectiveState>();
			states.Add(new ObjectiveState(0, "Started", ObjectiveStateType.Active));
			states.Add(new ObjectiveState(1, "Completed", ObjectiveStateType.Complete));
			ID = 0;
			if (idArray == null)
			{
				return;
			}
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public ObjectiveState GetState(int stateID)
		{
			foreach (ObjectiveState state in states)
			{
				if (state.ID == stateID)
				{
					return state;
				}
			}
			return null;
		}

		public string GetTitle(int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation(title, titleLineID, languageNumber);
		}

		public string GetDescription(int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation(description, descriptionLineID, languageNumber);
		}

		public string GetTranslatableString(int index)
		{
			if (index == 0)
			{
				return title;
			}
			return description;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return titleLineID;
			}
			return descriptionLineID;
		}
	}
}
