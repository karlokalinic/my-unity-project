using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharRename : Action, ITranslatable
	{
		public int _charID;

		public Char _char;

		public bool isPlayer;

		protected Char runtimeChar;

		public string newName;

		public int lineID = -1;

		public ActionCharRename()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Rename";
			lineID = -1;
			description = "Changes the display name of a Character when subtitles are used.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile(_charID, _char);
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}

		public override float Run()
		{
			if ((bool)runtimeChar && !string.IsNullOrEmpty(newName))
			{
				runtimeChar.SetName(newName, lineID);
			}
			return 0f;
		}

		public string GetTranslatableString(int index)
		{
			return newName;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}

		public static ActionCharRename CreateNew(Char characterToRename, string newName, int translationID = -1)
		{
			ActionCharRename actionCharRename = ScriptableObject.CreateInstance<ActionCharRename>();
			actionCharRename._char = characterToRename;
			actionCharRename.newName = newName;
			actionCharRename.lineID = translationID;
			return actionCharRename;
		}
	}
}
