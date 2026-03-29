using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionDocumentCollection : Action
	{
		public enum DocumentCollectionMethod
		{
			Add = 0,
			Remove = 1,
			Clear = 2
		}

		public int documentID;

		public int parameterID = -1;

		[SerializeField]
		protected DocumentCollectionMethod documentCollectionMethod;

		public ActionDocumentCollection()
		{
			isDisplayed = true;
			category = ActionCategory.Document;
			title = "Add or remove";
			description = "Adds or removes a document from the player's collection, or removes all of them.";
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
				if (documentCollectionMethod == DocumentCollectionMethod.Add)
				{
					KickStarter.runtimeDocuments.AddToCollection(document);
				}
				else if (documentCollectionMethod == DocumentCollectionMethod.Remove)
				{
					KickStarter.runtimeDocuments.RemoveFromCollection(document);
				}
				else if (documentCollectionMethod == DocumentCollectionMethod.Clear)
				{
					KickStarter.runtimeDocuments.ClearCollection();
				}
			}
			return 0f;
		}

		public static ActionDocumentCollection CreateNew(int documentID, DocumentCollectionMethod method)
		{
			ActionDocumentCollection actionDocumentCollection = ScriptableObject.CreateInstance<ActionDocumentCollection>();
			actionDocumentCollection.documentCollectionMethod = method;
			actionDocumentCollection.documentID = documentID;
			return actionDocumentCollection;
		}
	}
}
