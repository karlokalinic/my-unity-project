using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_documents.html")]
	public class RuntimeDocuments : MonoBehaviour
	{
		protected Document activeDocument;

		protected List<int> collectedDocuments = new List<int>();

		protected Dictionary<int, int> lastOpenPages = new Dictionary<int, int>();

		public Document ActiveDocument
		{
			get
			{
				return activeDocument;
			}
		}

		public void OnStart()
		{
			activeDocument = null;
			collectedDocuments.Clear();
			lastOpenPages.Clear();
			GetDocumentsOnStart();
		}

		public void OpenDocument(Document document)
		{
			if (document != null && activeDocument != document)
			{
				CloseDocument();
				activeDocument = document;
				KickStarter.eventManager.Call_OnHandleDocument(activeDocument, true);
			}
		}

		public void OpenDocument(int documentID)
		{
			if (documentID >= 0)
			{
				Document document = KickStarter.inventoryManager.GetDocument(documentID);
				OpenDocument(document);
			}
		}

		public void CloseDocument()
		{
			if (activeDocument != null)
			{
				KickStarter.eventManager.Call_OnHandleDocument(activeDocument, false);
				activeDocument = null;
			}
		}

		public bool DocumentIsInCollection(int ID)
		{
			if (collectedDocuments != null)
			{
				foreach (int collectedDocument in collectedDocuments)
				{
					if (collectedDocument == ID)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void AddToCollection(Document document)
		{
			if (!collectedDocuments.Contains(document.ID))
			{
				collectedDocuments.Add(document.ID);
				PlayerMenus.ResetInventoryBoxes();
			}
		}

		public void RemoveFromCollection(Document document)
		{
			if (collectedDocuments.Contains(document.ID))
			{
				collectedDocuments.Remove(document.ID);
				PlayerMenus.ResetInventoryBoxes();
			}
		}

		public void ClearCollection()
		{
			collectedDocuments.Clear();
			PlayerMenus.ResetInventoryBoxes();
		}

		public int GetLastOpenPage(Document document)
		{
			if (document.rememberLastOpenPage)
			{
				int value = 0;
				if (lastOpenPages.TryGetValue(document.ID, out value))
				{
					return value;
				}
			}
			return 1;
		}

		public void SetLastOpenPage(Document document, int page)
		{
			if (document.rememberLastOpenPage)
			{
				if (lastOpenPages.ContainsKey(document.ID))
				{
					lastOpenPages[document.ID] = page;
				}
				else
				{
					lastOpenPages.Add(document.ID, page);
				}
			}
		}

		public PlayerData SavePlayerDocuments(PlayerData playerData)
		{
			playerData.activeDocumentID = ((activeDocument == null) ? (-1) : activeDocument.ID);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int collectedDocument in collectedDocuments)
			{
				stringBuilder.Append(collectedDocument.ToString());
				stringBuilder.Append("|");
			}
			if (collectedDocuments.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			playerData.collectedDocumentData = stringBuilder.ToString();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (KeyValuePair<int, int> lastOpenPage in lastOpenPages)
			{
				stringBuilder2.Append(lastOpenPage.Key.ToString());
				stringBuilder2.Append(":");
				stringBuilder2.Append(lastOpenPage.Value.ToString());
				stringBuilder2.Append("|");
			}
			if (lastOpenPages.Count > 0)
			{
				stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
			}
			playerData.lastOpenDocumentPagesData = stringBuilder2.ToString();
			return playerData;
		}

		public void AssignPlayerDocuments(PlayerData playerData)
		{
			collectedDocuments.Clear();
			if (!string.IsNullOrEmpty(playerData.collectedDocumentData))
			{
				string[] array = playerData.collectedDocumentData.Split("|"[0]);
				string[] array2 = array;
				foreach (string s in array2)
				{
					int result = -1;
					if (int.TryParse(s, out result) && result >= 0)
					{
						collectedDocuments.Add(result);
					}
				}
			}
			lastOpenPages.Clear();
			if (!string.IsNullOrEmpty(playerData.lastOpenDocumentPagesData))
			{
				string[] array3 = playerData.lastOpenDocumentPagesData.Split("|"[0]);
				string[] array4 = array3;
				foreach (string text in array4)
				{
					string[] array5 = text.Split(":"[0]);
					int result2 = -1;
					if (int.TryParse(array5[0], out result2) && result2 >= 0)
					{
						int result3 = 1;
						if (int.TryParse(array5[1], out result3) && result3 > 1)
						{
							lastOpenPages.Add(result2, result3);
						}
					}
				}
			}
			OpenDocument(playerData.activeDocumentID);
		}

		protected void GetDocumentsOnStart()
		{
			if ((bool)KickStarter.inventoryManager)
			{
				foreach (Document document in KickStarter.inventoryManager.documents)
				{
					if (document.carryOnStart)
					{
						collectedDocuments.Add(document.ID);
					}
				}
				return;
			}
			ACDebug.LogError("No Inventory Manager found - please use the Adventure Creator window to create one.");
		}

		public int[] GetCollectedDocumentIDs(int[] limitToCategoryIDs = null)
		{
			if (limitToCategoryIDs != null && limitToCategoryIDs.Length >= 0)
			{
				List<int> list = new List<int>();
				foreach (int collectedDocument in collectedDocuments)
				{
					if (collectedDocument < 0)
					{
						continue;
					}
					Document document = KickStarter.inventoryManager.GetDocument(collectedDocument);
					bool flag = false;
					foreach (int num in limitToCategoryIDs)
					{
						if (document.binID == num)
						{
							flag = true;
						}
					}
					if (flag)
					{
						list.Add(collectedDocument);
					}
				}
				return list.ToArray();
			}
			return collectedDocuments.ToArray();
		}
	}
}
