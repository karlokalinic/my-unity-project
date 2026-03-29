using System;
using AC;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DocumentAudioPlayer : MonoBehaviour
{
	[Serializable]
	private class DocumentAudio
	{
		public int documentID;

		public PageAudio[] pageAudios = new PageAudio[0];
	}

	[Serializable]
	private class PageAudio
	{
		public AudioClip audioClip;

		public int speechLineID;
	}

	[SerializeField]
	private string documentMenuName = "Document";

	[SerializeField]
	private string documentElementName = "PageText";

	[SerializeField]
	private DocumentAudio[] documentAudios;

	private void OnEnable()
	{
		EventManager.OnDocumentOpen += OnOpenDocument;
		EventManager.OnMenuElementShift = (EventManager.Delegate_OnMenuElementShift)Delegate.Combine(EventManager.OnMenuElementShift, new EventManager.Delegate_OnMenuElementShift(OnMenuElementShift));
		EventManager.OnMenuTurnOff = (EventManager.Delegate_OnMenuTurnOn)Delegate.Combine(EventManager.OnMenuTurnOff, new EventManager.Delegate_OnMenuTurnOn(OnMenuTurnOff));
	}

	private void OnDisable()
	{
		EventManager.OnDocumentOpen -= OnOpenDocument;
		EventManager.OnMenuElementShift = (EventManager.Delegate_OnMenuElementShift)Delegate.Remove(EventManager.OnMenuElementShift, new EventManager.Delegate_OnMenuElementShift(OnMenuElementShift));
		EventManager.OnMenuTurnOff = (EventManager.Delegate_OnMenuTurnOn)Delegate.Remove(EventManager.OnMenuTurnOff, new EventManager.Delegate_OnMenuTurnOn(OnMenuTurnOff));
	}

	private void OnOpenDocument(DocumentInstance documentInstance)
	{
		PlayAudio();
	}

	private void OnMenuElementShift(MenuElement _element, AC_ShiftInventory shiftType)
	{
		if (_element is MenuJournal)
		{
			PlayAudio();
		}
	}

	private void OnMenuTurnOff(Menu _menu, bool isInstant)
	{
		if (_menu.title == documentMenuName)
		{
			StopAudio();
		}
	}

	private void PlayAudio()
	{
		MenuJournal menuJournal = PlayerMenus.GetElementWithName(documentMenuName, documentElementName) as MenuJournal;
		Document activeDocument = KickStarter.runtimeDocuments.ActiveDocument;
		if (activeDocument != null)
		{
			int pageIndex = menuJournal.showPage - 1;
			PlayAudio(activeDocument.ID, pageIndex);
		}
	}

	private void PlayAudio(int documentID, int pageIndex)
	{
		DocumentAudio[] array = documentAudios;
		foreach (DocumentAudio documentAudio in array)
		{
			if (documentAudio.documentID == documentID && pageIndex >= 0 && pageIndex < documentAudio.pageAudios.Length)
			{
				PageAudio pageAudio = documentAudio.pageAudios[pageIndex];
				KickStarter.dialog.GetNarratorAudioSource().ignoreListenerPause = true;
				KickStarter.dialog.StartDialog(null, pageAudio.speechLineID, true, true);
			}
		}
	}

	private void StopAudio()
	{
		KickStarter.dialog.EndSpeechByCharacter(null);
	}
}
