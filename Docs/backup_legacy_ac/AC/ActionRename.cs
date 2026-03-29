using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionRename : Action, ITranslatable
	{
		public int constantID;

		public int parameterID = -1;

		public Hotspot hotspot;

		protected Hotspot runtimeHotspot;

		public string newName;

		public int lineID = -1;

		public ActionRename()
		{
			isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Rename";
			lineID = -1;
			description = "Renames a Hotspot, or an NPC with a Hotspot component.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile(parameters, parameterID, constantID, hotspot);
		}

		public override float Run()
		{
			if ((bool)runtimeHotspot && !string.IsNullOrEmpty(newName))
			{
				runtimeHotspot.SetName(newName, lineID);
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

		public static ActionRename CreateNew(Hotspot hotspotToRename, string newName, int translationID = -1)
		{
			ActionRename actionRename = ScriptableObject.CreateInstance<ActionRename>();
			actionRename.hotspot = hotspotToRename;
			actionRename.newName = newName;
			actionRename.lineID = translationID;
			return actionRename;
		}
	}
}
