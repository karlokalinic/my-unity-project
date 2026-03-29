using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionDocumentOpen : Action
	{
		public int documentID;

		public int parameterID = -1;

		public bool addToCollection;

		public ActionDocumentOpen()
		{
			isDisplayed = true;
			category = ActionCategory.Document;
			title = "Open";
			description = "Opens a document, causing any Menu of 'Appear type: On View Document' to open.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			documentID = AssignDocumentID(parameters, parameterID, documentID);
		}

		public override float Run()
		{
			Document document = KickStarter.inventoryManager.GetDocument(documentID);
			if (document != null)
			{
				if (addToCollection)
				{
					KickStarter.runtimeDocuments.AddToCollection(document);
				}
				KickStarter.runtimeDocuments.OpenDocument(document);
			}
			return 0f;
		}

		public static ActionDocumentOpen CreateNew(int documentID, bool addToCollection)
		{
			ActionDocumentOpen actionDocumentOpen = ScriptableObject.CreateInstance<ActionDocumentOpen>();
			actionDocumentOpen.documentID = documentID;
			actionDocumentOpen.addToCollection = addToCollection;
			return actionDocumentOpen;
		}
	}
}
