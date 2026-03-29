using AC;
using UnityEngine;

public class RunActionListWhenCloseDocument : MonoBehaviour
{
	public int documentID;

	public ActionList actionlistOnClose;

	private void OnEnable()
	{
		EventManager.OnDocumentClose += OnCloseDocument;
	}

	private void OnDisable()
	{
		EventManager.OnDocumentClose -= OnCloseDocument;
	}

	private void OnCloseDocument(DocumentInstance documentInstance)
	{
		if (!DocumentInstance.IsValid(documentInstance))
		{
			return;
		}

		Debug.Log("Closed document: " + documentInstance.DocumentID);
		if (documentInstance.DocumentID == documentID)
		{
			actionlistOnClose.Interact();
		}
	}
}
