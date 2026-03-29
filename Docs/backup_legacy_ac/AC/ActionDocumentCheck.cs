using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionDocumentCheck : ActionCheck
	{
		public int documentID;

		public int parameterID = -1;

		public ActionDocumentCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Document;
			title = "Check";
			description = "Checks to see if a particular Document is in the Player's possession.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			documentID = AssignDocumentID(parameters, parameterID, documentID);
		}

		public override bool CheckCondition()
		{
			return KickStarter.runtimeDocuments.DocumentIsInCollection(documentID);
		}

		public static ActionDocumentCheck CreateNew(int documentID)
		{
			ActionDocumentCheck actionDocumentCheck = ScriptableObject.CreateInstance<ActionDocumentCheck>();
			actionDocumentCheck.documentID = documentID;
			return actionDocumentCheck;
		}
	}
}
