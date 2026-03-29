using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AC
{
	public class EventManager : MonoBehaviour
	{
		public delegate void Delegate_StartSpeech(Char speakingCharacter, string speechText, int lineID);

		public delegate void Delegate_StopSpeech(Char speakingCharacter);

		public delegate void Delegate_Speech(Speech speech);

		public delegate void Delegate_SpeechToken(Char speakingCharacter, int lineID, string tokenKey, string tokenValue);

		public delegate void Delegate_SpeechTokenAlt(Speech speech, string tokenKey, string tokenValue);

		public delegate string Delegate_OnRequestSpeechTokenReplacement(Speech speech, string tokenKey, string tokenValue);

		public delegate string Delegate_OnRequestTextTokenReplacement(string tokenKey, string tokenValue);

		public delegate void Delegate_OnLoadSpeechAssetBundle(int language);

		public delegate void Delegate_ChangeGameState(GameState gameState);

		public delegate void Delegate_Conversation(Conversation conversation);

		public delegate void Delegate_ConversationChoice(Conversation conversation, int optionID);

		public delegate void Delegate_ChangeHotspot(Hotspot hotspot);

		public delegate void Delegate_InteractHotspot(Hotspot hotspot, Button button);

		public delegate List<Hotspot> Delegate_HotspotCollection(DetectHotspots hotspotDetector, List<Hotspot> hotspots);

		public delegate void Delegate_OnRunTrigger(AC_Trigger trigger, GameObject collidingObject);

		public delegate void Delegate_OnVariableChange(GVar variable);

		public delegate void Delegate_OnVariableUpload(GVar variable, Variables variables);

		public delegate void Delegate_OnMenuElementClick(Menu _menu, MenuElement _element, int _slot, int buttonPressed);

		public delegate void Delegate_OnMouseOverMenu(Menu _menu, MenuElement _element, int _slot);

		public delegate void Delegate_OnMenuElementVisiblity(MenuElement _element);

		public delegate void Delegate_OnMenuElementShift(MenuElement _element, AC_ShiftInventory shiftType);

		public delegate void Delegate_OnMenuTurnOn(Menu _menu, bool isInstant);

		public delegate void Delegate_OnUpdateDragLine(Vector2 startScreenPosition, Vector2 endScreenPosition);

		public delegate void Delegate_OnEnableInteractionMenus(Hotspot hotspot, InvItem invItem);

		public delegate void Delegate_OnModifyJournalPage(MenuJournal journal, JournalPage page, int index);

		public delegate string Delegate_OnRequestMenuElementHotspotLabel(Menu _menu, MenuElement _element, int _slot, int _language);

		public delegate void Delegate_OnChangeCursorMode(int cursorID);

		public delegate void Delegate_OnSetHardwareCursor(Texture2D cursorTexture, Vector2 clickOffset);

		public delegate void Delegate_Generic();

		public delegate void Delegate_SaveFile(SaveFile saveFile);

		public delegate void Delegate_SaveID(int saveID);

		public delegate void Delegate_OnSwitchProfile(int profileID);

		public delegate void Delegate_SetPlayer(Player player);

		public delegate void Delegate_OnCharacterTimeline(Char character, PlayableDirector director, int trackIndex);

		public delegate void Delegate_OnCharacterEndPath(Char character, Paths path);

		public delegate void Delegate_OnCharacterSetPath(Char character, Paths path);

		public delegate void Delegate_OnCharacterReachNode(Char character, Paths path, int node);

		public delegate void Delegate_SetHeadTurnTarget(Char character, Transform headTurnTarget, Vector3 targetOffset, bool isInstant);

		public delegate void Delegate_ClearHeadTurnTarget(Char character, bool isInstant);

		public delegate void Delegate_OnOccupyPlayerStart(Player player, PlayerStart playerStart);

		public delegate void Delegate_OnPointAndClick(Vector3[] pointArray, bool run);

		public delegate void Delegate_ChangeInventory(InvItem invItem, int value);

		public delegate void Delegate_CombineInventory(InvItem invItem, InvItem invItem2);

		public delegate void Delegate_Inventory(InvItem _int);

		public delegate void Delegate_Container(Container container, ContainerItem containerItem);

		public delegate void Delegate_Crafting(Recipe recipe);

		public delegate void Delegate_OnMoveable(DragBase dragBase);

		public delegate void Delegate_OnDraggableSnap(DragBase dragBase, DragTrack track, TrackSnapData trackSnapData);

		public delegate void Delegate_OnSwitchCamera(_Camera fromCamera, _Camera toCamera, float transitionTime);

		public delegate void Delegate_OnShakeCamera(float intensity, float duration);

		public delegate void Delegate_OnChangeLanguage(int language);

		public delegate void Delegate_OnChangeVolume(SoundType soundType, float volume);

		public delegate void Delegate_OnChangeSubtitles(bool showSubtitles);

		public delegate void Delegate_NoParameters();

		public delegate void Delegate_AfterSceneChange(LoadingGame loadingGame);

		public delegate void Delegate_OnCompleteScenePreload(SceneInfo sceneInfo);

		public delegate void Delegate_HandleDocument(Document document);

		public delegate void Delegate_HandleObjective(Objective objective, ObjectiveState state);

		public delegate void Delegate_OnPlaySoundtrack(int trackID, bool loop, float fadeTime, int startingSample);

		public delegate void Delegate_OnStopSoundtrack(float fadeTime);

		public delegate void Delegate_PlayFootstepSound(Char character, FootstepSounds footstepSounds, bool isWalkingSound, AudioSource audioSource, AudioClip audioClip);

		public delegate void Delegate_OnHandleSound(Sound sound, AudioSource audioSource, AudioClip audioClip, float fadeTime);

		public delegate void Delegate_OnBeginActionList(ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping);

		public delegate void Delegate_OnEndActionList(ActionList actionList, ActionListAsset actionListAsset, bool isSkipping);

		public delegate void Delegate_OnPauseActionList(ActionList actionList);

		public static Delegate_ChangeHotspot OnHotspotSelect;

		public static Delegate_ChangeHotspot OnHotspotDeselect;

		public static Delegate_InteractHotspot OnHotspotInteract;

		public static Delegate_ChangeHotspot OnDoubleClickHotspot;

		public static Delegate_ChangeHotspot OnHotspotTurnOn;

		public static Delegate_ChangeHotspot OnHotspotTurnOff;

		public static Delegate_ChangeHotspot OnHotspotStopMovingTo;

		public static Delegate_HotspotCollection OnModifyHotspotDetectorCollection;

		public static Delegate_ChangeHotspot OnRegisterHotspot;

		public static Delegate_ChangeHotspot OnUnregisterHotspot;

		public static Delegate_OnRunTrigger OnRunTrigger;

		public static Delegate_OnVariableChange OnVariableChange;

		public static Delegate_OnVariableUpload OnUploadVariable;

		public static Delegate_OnVariableUpload OnDownloadVariable;

		public static Delegate_OnMenuElementClick OnMenuElementClick;

		public static Delegate_OnMouseOverMenu OnMouseOverMenu;

		public static Delegate_OnMenuElementVisiblity OnMenuElementShow;

		public static Delegate_OnMenuElementVisiblity OnMenuElementHide;

		public static Delegate_OnMenuElementShift OnMenuElementShift;

		public static Delegate_Generic OnGenerateMenus;

		public static Delegate_OnMenuTurnOn OnMenuTurnOn;

		public static Delegate_OnMenuTurnOn OnMenuTurnOff;

		public static Delegate_OnUpdateDragLine OnUpdateDragLine;

		public static Delegate_OnEnableInteractionMenus OnEnableInteractionMenus;

		public static Delegate_OnModifyJournalPage OnJournalPageAdd;

		public static Delegate_OnModifyJournalPage OnJournalPageRemove;

		public static Delegate_OnRequestMenuElementHotspotLabel OnRequestMenuElementHotspotLabel;

		public static Delegate_OnChangeCursorMode OnChangeCursorMode;

		public static Delegate_OnSetHardwareCursor OnSetHardwareCursor;

		public static Delegate_SaveID OnBeforeSaving;

		public static Delegate_SaveFile OnFinishSaving;

		public static Delegate_SaveID OnFailSaving;

		public static Delegate_SaveFile OnBeforeLoading;

		public static Delegate_Generic OnFinishLoading;

		public static Delegate_SaveID OnFailLoading;

		public static Delegate_Generic OnBeforeImporting;

		public static Delegate_Generic OnFinishImporting;

		public static Delegate_Generic OnFailImporting;

		public static Delegate_OnSwitchProfile OnSwitchProfile;

		public static Delegate_Generic OnRestartGame;

		public static Delegate_SetPlayer OnSetPlayer;

		public static Delegate_OnCharacterTimeline OnCharacterEnterTimeline;

		public static Delegate_OnCharacterTimeline OnCharacterExitTimeline;

		public static Delegate_OnCharacterEndPath OnCharacterEndPath;

		public static Delegate_OnCharacterSetPath OnCharacterSetPath;

		public static Delegate_OnCharacterReachNode OnCharacterReachNode;

		public static Delegate_SetHeadTurnTarget OnSetHeadTurnTarget;

		public static Delegate_ClearHeadTurnTarget OnClearHeadTurnTarget;

		public static Delegate_OnOccupyPlayerStart OnOccupyPlayerStart;

		public static Delegate_OnPointAndClick OnPointAndClick;

		public static Delegate_ChangeInventory OnInventoryAdd;

		public static Delegate_ChangeInventory OnInventoryRemove;

		public static Delegate_Inventory OnInventorySelect;

		public static Delegate_Inventory OnInventoryDeselect;

		public static Delegate_ChangeInventory OnInventoryInteract;

		public static Delegate_CombineInventory OnInventoryCombine;

		public static Delegate_Container OnContainerAdd;

		public static Delegate_Container OnContainerRemove;

		public static Delegate_Container OnContainerRemoveFail;

		public static Delegate_Crafting OnCraftingSucceed;

		public static Delegate_PlayFootstepSound OnPlayFootstepSound;

		public static event Delegate_StartSpeech OnStartSpeech;

		public static event Delegate_Speech OnStartSpeech_Alt;

		public static event Delegate_StopSpeech OnStopSpeech;

		public static event Delegate_Speech OnStopSpeech_Alt;

		public static event Delegate_StartSpeech OnStartSpeechScroll;

		public static event Delegate_Speech OnStartSpeechScroll_Alt;

		public static event Delegate_StartSpeech OnEndSpeechScroll;

		public static event Delegate_Speech OnEndSpeechScroll_Alt;

		public static event Delegate_StartSpeech OnCompleteSpeechScroll;

		public static event Delegate_Speech OnCompleteSpeechScroll_Alt;

		public static event Delegate_SpeechToken OnSpeechToken;

		public static event Delegate_SpeechTokenAlt OnSpeechToken_Alt;

		public static event Delegate_OnRequestSpeechTokenReplacement OnRequestSpeechTokenReplacement;

		public static event Delegate_OnRequestTextTokenReplacement OnRequestTextTokenReplacement;

		public static event Delegate_OnLoadSpeechAssetBundle OnLoadSpeechAssetBundle;

		public static event Delegate_ChangeGameState OnEnterGameState;

		public static event Delegate_ChangeGameState OnExitGameState;

		public static event Delegate_Conversation OnStartConversation;

		public static event Delegate_ConversationChoice OnClickConversation;

		public static event Delegate_OnMoveable OnGrabMoveable;

		public static event Delegate_OnMoveable OnDropMoveable;

		public static event Delegate_OnDraggableSnap OnDraggableSnap;

		public static event Delegate_OnSwitchCamera OnSwitchCamera;

		public static event Delegate_OnShakeCamera OnShakeCamera;

		public static event Delegate_Generic OnUpdatePlayableScreenArea;

		public static event Delegate_OnChangeLanguage OnChangeLanguage;

		public static event Delegate_OnChangeLanguage OnChangeVoiceLanguage;

		public static event Delegate_OnChangeVolume OnChangeVolume;

		public static event Delegate_OnChangeSubtitles OnChangeSubtitles;

		public static event Delegate_NoParameters OnBeforeChangeScene;

		public static event Delegate_AfterSceneChange OnAfterChangeScene;

		public static event Delegate_NoParameters OnStartScene;

		public static event Delegate_OnCompleteScenePreload OnCompleteScenePreload;

		public static event Delegate_OnCompleteScenePreload OnAwaitSceneActivation;

		public static event Delegate_HandleDocument OnOpenDocument;

		public static event Delegate_HandleDocument OnCloseDocument;

		public static event Delegate_HandleObjective OnObjectiveUpdate;

		public static event Delegate_HandleObjective OnObjectiveSelect;

		public static event Delegate_OnPlaySoundtrack OnPlayMusic;

		public static event Delegate_OnPlaySoundtrack OnPlayAmbience;

		public static event Delegate_OnStopSoundtrack OnStopMusic;

		public static event Delegate_OnStopSoundtrack OnStopAmbience;

		public static event Delegate_OnHandleSound OnPlaySound;

		public static event Delegate_OnHandleSound OnStopSound;

		public static event Delegate_OnBeginActionList OnBeginActionList;

		public static event Delegate_OnEndActionList OnEndActionList;

		public static event Delegate_OnPauseActionList OnPauseActionList;

		public static event Delegate_OnPauseActionList OnResumeActionList;

		public void Call_OnStartSpeech(Speech speech, Char speakingCharacter, string speechText, int lineID)
		{
			if (EventManager.OnStartSpeech != null)
			{
				EventManager.OnStartSpeech(speakingCharacter, speechText, lineID);
			}
			if (EventManager.OnStartSpeech_Alt != null)
			{
				EventManager.OnStartSpeech_Alt(speech);
			}
		}

		public void Call_OnStopSpeech(Speech speech, Char speakingCharacter)
		{
			if (EventManager.OnStopSpeech != null)
			{
				EventManager.OnStopSpeech(speakingCharacter);
			}
			if (EventManager.OnStopSpeech_Alt != null)
			{
				EventManager.OnStopSpeech_Alt(speech);
			}
		}

		public void Call_OnStartSpeechScroll(Speech speech, Char speakingCharacter, string speechText, int lineID)
		{
			if (EventManager.OnStartSpeechScroll != null)
			{
				EventManager.OnStartSpeechScroll(speakingCharacter, speechText, lineID);
			}
			if (EventManager.OnStartSpeechScroll_Alt != null)
			{
				EventManager.OnStartSpeechScroll_Alt(speech);
			}
		}

		public void Call_OnEndSpeechScroll(Speech speech, Char speakingCharacter, string speechText, int lineID)
		{
			if (EventManager.OnEndSpeechScroll != null)
			{
				EventManager.OnEndSpeechScroll(speakingCharacter, speechText, lineID);
			}
			if (EventManager.OnEndSpeechScroll_Alt != null)
			{
				EventManager.OnEndSpeechScroll_Alt(speech);
			}
		}

		public void Call_OnCompleteSpeechScroll(Speech speech, Char speakingCharacter, string speechText, int lineID)
		{
			if (EventManager.OnCompleteSpeechScroll != null)
			{
				EventManager.OnCompleteSpeechScroll(speakingCharacter, speechText, lineID);
			}
			if (EventManager.OnCompleteSpeechScroll_Alt != null)
			{
				EventManager.OnCompleteSpeechScroll_Alt(speech);
			}
		}

		public void Call_OnSpeechToken(Speech speech, string tokenKey, string tokenValue)
		{
			if (EventManager.OnSpeechToken != null)
			{
				EventManager.OnSpeechToken(speech.speaker, speech.log.lineID, tokenKey, tokenValue);
			}
			if (EventManager.OnSpeechToken_Alt != null)
			{
				EventManager.OnSpeechToken_Alt(speech, tokenKey, tokenValue);
			}
		}

		public string Call_OnRequestSpeechTokenReplacement(Speech speech, string tokenKey, string tokenValue)
		{
			if (EventManager.OnRequestSpeechTokenReplacement != null)
			{
				return EventManager.OnRequestSpeechTokenReplacement(speech, tokenKey, tokenValue);
			}
			return string.Empty;
		}

		public string Call_OnRequestTextTokenReplacement(string tokenKey, string tokenValue)
		{
			if (EventManager.OnRequestTextTokenReplacement != null)
			{
				return EventManager.OnRequestTextTokenReplacement(tokenKey, tokenValue);
			}
			return string.Empty;
		}

		public void Call_OnLoadSpeechAssetBundle(int language)
		{
			if (EventManager.OnLoadSpeechAssetBundle != null)
			{
				EventManager.OnLoadSpeechAssetBundle(language);
			}
		}

		public void Call_OnChangeGameState(GameState oldGameState)
		{
			if (EventManager.OnExitGameState != null)
			{
				EventManager.OnExitGameState(oldGameState);
			}
			if (EventManager.OnEnterGameState != null)
			{
				EventManager.OnEnterGameState(KickStarter.stateHandler.gameState);
			}
		}

		public void Call_OnStartConversation(Conversation conversation)
		{
			if (EventManager.OnStartConversation != null)
			{
				EventManager.OnStartConversation(conversation);
			}
		}

		public void Call_OnClickConversation(Conversation conversation, int optionID)
		{
			if (EventManager.OnClickConversation != null)
			{
				EventManager.OnClickConversation(conversation, optionID);
			}
		}

		public void Call_OnChangeHotspot(Hotspot hotspot, bool wasSelected)
		{
			if (!(hotspot == null))
			{
				if (wasSelected && OnHotspotSelect != null)
				{
					OnHotspotSelect(hotspot);
				}
				else if (!wasSelected && OnHotspotDeselect != null)
				{
					OnHotspotDeselect(hotspot);
				}
			}
		}

		public void Call_OnInteractHotspot(Hotspot hotspot, Button button)
		{
			if (!(hotspot == null) && OnHotspotInteract != null)
			{
				OnHotspotInteract(hotspot, button);
			}
		}

		public void Call_OnDoubleClickHotspot(Hotspot hotspot)
		{
			if (!(hotspot == null) && OnDoubleClickHotspot != null)
			{
				OnDoubleClickHotspot(hotspot);
			}
		}

		public void Call_OnTurnHotspot(Hotspot hotspot, bool isOn)
		{
			if (hotspot == null)
			{
				return;
			}
			if (isOn)
			{
				if (OnHotspotTurnOn != null)
				{
					OnHotspotTurnOn(hotspot);
				}
			}
			else if (OnHotspotTurnOff != null)
			{
				OnHotspotTurnOff(hotspot);
			}
		}

		public void Call_OnHotspotStopMovingTo(Hotspot hotspot)
		{
			if (!(hotspot == null) && OnHotspotStopMovingTo != null)
			{
				OnHotspotStopMovingTo(hotspot);
			}
		}

		public List<Hotspot> Call_OnModifyHotspotDetectorCollection(DetectHotspots hotspotDetector, List<Hotspot> hotspots)
		{
			if (hotspots == null)
			{
				return null;
			}
			if (OnModifyHotspotDetectorCollection != null)
			{
				return OnModifyHotspotDetectorCollection(hotspotDetector, hotspots);
			}
			return hotspots;
		}

		public void Call_OnRegisterHotspot(Hotspot hotspot, bool wasRegistered)
		{
			if (wasRegistered)
			{
				if (OnRegisterHotspot != null)
				{
					OnRegisterHotspot(hotspot);
				}
			}
			else if (OnUnregisterHotspot != null)
			{
				OnUnregisterHotspot(hotspot);
			}
		}

		public void Call_OnRunTrigger(AC_Trigger trigger, GameObject collidingObject)
		{
			if (!(trigger == null) && !(collidingObject == null) && OnRunTrigger != null)
			{
				OnRunTrigger(trigger, collidingObject);
			}
		}

		public void Call_OnVariableChange(GVar _variable)
		{
			if (OnVariableChange != null)
			{
				OnVariableChange(_variable);
			}
		}

		public void Call_OnDownloadVariable(GVar _variable, Variables variables = null)
		{
			if (OnDownloadVariable != null)
			{
				OnDownloadVariable(_variable, variables);
			}
		}

		public void Call_OnUploadVariable(GVar _variable, Variables variables = null)
		{
			if (OnUploadVariable != null)
			{
				OnUploadVariable(_variable, variables);
			}
		}

		public void Call_OnMenuElementClick(Menu _menu, MenuElement _element, int _slot, int _buttonPressed)
		{
			if (OnMenuElementClick != null)
			{
				OnMenuElementClick(_menu, _element, _slot, _buttonPressed);
			}
		}

		public void Call_OnMouseOverMenuElement(Menu _menu, MenuElement _element, int _slot)
		{
			if (OnMouseOverMenu != null)
			{
				OnMouseOverMenu(_menu, _element, _slot);
			}
		}

		public void Call_OnMenuElementChangeVisibility(MenuElement _element)
		{
			if (_element.IsVisible)
			{
				if (OnMenuElementShow != null)
				{
					OnMenuElementShow(_element);
				}
			}
			else if (OnMenuElementHide != null)
			{
				OnMenuElementHide(_element);
			}
		}

		public void Call_OnMenuElementShift(MenuElement _element, AC_ShiftInventory shiftType)
		{
			if (OnMenuElementShift != null)
			{
				OnMenuElementShift(_element, shiftType);
			}
		}

		public void Call_OnGenerateMenus()
		{
			if (OnGenerateMenus != null)
			{
				OnGenerateMenus();
			}
		}

		public void Call_OnMenuTurnOn(Menu _menu, bool isInstant)
		{
			if (OnMenuTurnOn != null)
			{
				OnMenuTurnOn(_menu, isInstant);
			}
		}

		public void Call_OnMenuTurnOff(Menu _menu, bool isInstant)
		{
			if (OnMenuTurnOff != null)
			{
				OnMenuTurnOff(_menu, isInstant);
			}
		}

		public void Call_OnUpdateDragLine(Vector2 startScreenPosition, Vector2 endScreenPosition)
		{
			if (OnUpdateDragLine != null)
			{
				OnUpdateDragLine(startScreenPosition, endScreenPosition);
			}
		}

		public void Call_OnEnableInteractionMenus(Hotspot hotspot, InvItem invItem)
		{
			if (OnEnableInteractionMenus != null)
			{
				OnEnableInteractionMenus(hotspot, invItem);
			}
		}

		public void Call_OnModifyJournalPage(MenuJournal journal, JournalPage page, int index, bool wasAdded)
		{
			if (wasAdded)
			{
				if (OnJournalPageAdd != null)
				{
					OnJournalPageAdd(journal, page, index);
				}
			}
			else if (OnJournalPageRemove != null)
			{
				OnJournalPageRemove(journal, page, index);
			}
		}

		public string Call_OnRequestMenuElementHotspotLabel(Menu _menu, MenuElement _element, int _slot, int language)
		{
			if (OnRequestMenuElementHotspotLabel != null)
			{
				return OnRequestMenuElementHotspotLabel(_menu, _element, _slot, language);
			}
			return string.Empty;
		}

		public void Call_OnChangeCursorMode(int cursorID)
		{
			if (OnChangeCursorMode != null)
			{
				OnChangeCursorMode(cursorID);
			}
		}

		public void Call_OnSetHardwareCursor(Texture2D cursorTexture, Vector2 clickOffset)
		{
			if (OnSetHardwareCursor != null)
			{
				OnSetHardwareCursor(cursorTexture, clickOffset);
			}
		}

		public void Call_OnSave(FileAccessState fileAccessState, int saveID, SaveFile saveFile = null)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeSaving != null)
			{
				OnBeforeSaving(saveID);
			}
			else if (fileAccessState == FileAccessState.After && OnFinishSaving != null)
			{
				OnFinishSaving(saveFile);
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailSaving != null)
			{
				OnFailSaving(saveID);
			}
		}

		public void Call_OnLoad(FileAccessState fileAccessState, int saveID, SaveFile saveFile = null)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeLoading != null)
			{
				OnBeforeLoading(saveFile);
			}
			else if (fileAccessState == FileAccessState.After && OnFinishLoading != null)
			{
				OnFinishLoading();
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailLoading != null)
			{
				OnFailLoading(saveID);
			}
		}

		public void Call_OnImport(FileAccessState fileAccessState)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeImporting != null)
			{
				OnBeforeImporting();
			}
			else if (fileAccessState == FileAccessState.After && OnFinishImporting != null)
			{
				OnFinishImporting();
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailImporting != null)
			{
				OnFailImporting();
			}
		}

		public void Call_OnSwitchProfile(int profileID)
		{
			if (OnSwitchProfile != null)
			{
				OnSwitchProfile(profileID);
			}
		}

		public void Call_OnRestartGame()
		{
			if (OnRestartGame != null)
			{
				OnRestartGame();
			}
		}

		public void Call_OnSetPlayer(Player player)
		{
			if (OnSetPlayer != null)
			{
				OnSetPlayer(player);
			}
		}

		public void Call_OnCharacterTimeline(Char character, PlayableDirector director, int trackIndex, bool isEntering)
		{
			if (!(character != null))
			{
				return;
			}
			if (isEntering)
			{
				if (OnCharacterEnterTimeline != null)
				{
					OnCharacterEnterTimeline(character, director, trackIndex);
				}
			}
			else if (OnCharacterExitTimeline != null)
			{
				OnCharacterExitTimeline(character, director, trackIndex);
			}
		}

		public void Call_OnSetHeadTurnTarget(Char character, Transform headTurnTarget, Vector3 targetOffset, bool isInstant)
		{
			if (OnSetHeadTurnTarget != null)
			{
				OnSetHeadTurnTarget(character, headTurnTarget, targetOffset, isInstant);
			}
		}

		public void Call_OnClearHeadTurnTarget(Char character, bool isInstant)
		{
			if (OnClearHeadTurnTarget != null)
			{
				OnClearHeadTurnTarget(character, isInstant);
			}
		}

		public void Call_OnCharacterEndPath(Char character, Paths path)
		{
			if (OnCharacterEndPath != null)
			{
				OnCharacterEndPath(character, path);
			}
		}

		public void Call_OnCharacterSetPath(Char character, Paths path)
		{
			if (OnCharacterSetPath != null)
			{
				OnCharacterSetPath(character, path);
			}
		}

		public void Call_OnCharacterReachNode(Char character, Paths path, int node)
		{
			if (OnCharacterReachNode != null)
			{
				OnCharacterReachNode(character, path, node);
			}
		}

		public void Call_OnOccupyPlayerStart(Player player, PlayerStart playerStart)
		{
			if (OnOccupyPlayerStart != null)
			{
				OnOccupyPlayerStart(player, playerStart);
			}
		}

		public void Call_OnPointAndClick(Vector3[] pointArray, bool run)
		{
			if (OnPointAndClick != null)
			{
				OnPointAndClick(pointArray, run);
			}
		}

		public void Call_OnChangeInventory(InvItem invItem, InventoryEventType inventoryEventType, int amount = 1)
		{
			if (invItem != null)
			{
				if (inventoryEventType == InventoryEventType.Add && OnInventoryAdd != null)
				{
					OnInventoryAdd(invItem, amount);
				}
				else if (inventoryEventType == InventoryEventType.Remove && OnInventoryRemove != null)
				{
					OnInventoryRemove(invItem, amount);
				}
				else if (inventoryEventType == InventoryEventType.Select && OnInventorySelect != null)
				{
					OnInventorySelect(invItem);
				}
				else if (inventoryEventType == InventoryEventType.Deselect && OnInventoryDeselect != null)
				{
					OnInventoryDeselect(invItem);
				}
			}
		}

		public void Call_OnUseInventory(InvItem invItem, int iconID, InvItem combineItem = null)
		{
			if (invItem != null)
			{
				if (OnInventoryCombine != null && combineItem != null)
				{
					OnInventoryCombine(invItem, combineItem);
				}
				else if (OnInventoryInteract != null && combineItem == null)
				{
					OnInventoryInteract(invItem, iconID);
				}
			}
		}

		public void Call_OnUseContainer(bool transferringToContainer, Container container, ContainerItem containerItem)
		{
			if (containerItem == null || container == null)
			{
				return;
			}
			if (transferringToContainer)
			{
				if (OnContainerAdd != null)
				{
					OnContainerAdd(container, containerItem);
				}
			}
			else if (OnContainerRemove != null)
			{
				OnContainerRemove(container, containerItem);
			}
		}

		public void Call_OnUseContainerFail(Container container, ContainerItem containerItem)
		{
			if (containerItem != null && !(container == null) && OnContainerRemoveFail != null)
			{
				OnContainerRemoveFail(container, containerItem);
			}
		}

		public void Call_OnCraftingSucceed(Recipe recipe)
		{
			if (OnCraftingSucceed != null)
			{
				OnCraftingSucceed(recipe);
			}
		}

		public void Call_OnGrabMoveable(DragBase dragBase)
		{
			if (EventManager.OnGrabMoveable != null)
			{
				EventManager.OnGrabMoveable(dragBase);
			}
		}

		public void Call_OnDropMoveable(DragBase dragBase)
		{
			if (EventManager.OnDropMoveable != null)
			{
				EventManager.OnDropMoveable(dragBase);
			}
		}

		public void Call_OnDraggableSnap(DragBase dragBase, DragTrack track, TrackSnapData trackSnapData)
		{
			if (EventManager.OnDraggableSnap != null)
			{
				EventManager.OnDraggableSnap(dragBase, track, trackSnapData);
			}
		}

		public void Call_OnSwitchCamera(_Camera fromCamera, _Camera toCamera, float transitionTime)
		{
			if (EventManager.OnSwitchCamera != null)
			{
				EventManager.OnSwitchCamera(fromCamera, toCamera, transitionTime);
			}
		}

		public void Call_OnShakeCamera(float intensity, float duration)
		{
			if (EventManager.OnShakeCamera != null)
			{
				EventManager.OnShakeCamera(intensity, duration);
			}
		}

		public void Call_OnUpdatePlayableScreenArea()
		{
			if (EventManager.OnUpdatePlayableScreenArea != null)
			{
				EventManager.OnUpdatePlayableScreenArea();
			}
		}

		public void Call_OnChangeLanguage(int language)
		{
			if (EventManager.OnChangeLanguage != null)
			{
				EventManager.OnChangeLanguage(language);
			}
		}

		public void Call_OnChangeVoiceLanguage(int voiceLanguage)
		{
			if (EventManager.OnChangeVoiceLanguage != null)
			{
				EventManager.OnChangeVoiceLanguage(voiceLanguage);
			}
		}

		public void Call_OnChangeVolume(SoundType soundType, float volume)
		{
			if (EventManager.OnChangeVolume != null)
			{
				EventManager.OnChangeVolume(soundType, volume);
			}
		}

		public void Call_OnChangeSubtitles(bool showSubtitles)
		{
			if (EventManager.OnChangeSubtitles != null)
			{
				EventManager.OnChangeSubtitles(showSubtitles);
			}
		}

		public void Call_OnBeforeChangeScene()
		{
			if (EventManager.OnBeforeChangeScene != null)
			{
				EventManager.OnBeforeChangeScene();
			}
		}

		public void Call_OnAfterChangeScene(LoadingGame loadingGame)
		{
			if (EventManager.OnAfterChangeScene != null)
			{
				EventManager.OnAfterChangeScene(loadingGame);
			}
		}

		public void Call_OnStartScene()
		{
			if (EventManager.OnStartScene != null)
			{
				EventManager.OnStartScene();
			}
		}

		public void Call_OnCompleteScenePreload(SceneInfo preloadedSceneInfo)
		{
			if (EventManager.OnCompleteScenePreload != null)
			{
				EventManager.OnCompleteScenePreload(preloadedSceneInfo);
			}
		}

		public void Call_OnAwaitSceneActivation(SceneInfo nextSceneInfo)
		{
			if (EventManager.OnAwaitSceneActivation != null)
			{
				EventManager.OnAwaitSceneActivation(nextSceneInfo);
			}
		}

		public void Call_OnHandleDocument(Document document, bool isOpening)
		{
			if (isOpening)
			{
				if (EventManager.OnOpenDocument != null)
				{
					EventManager.OnOpenDocument(document);
				}
			}
			else if (EventManager.OnCloseDocument != null)
			{
				EventManager.OnCloseDocument(document);
			}
		}

		public void Call_OnObjectiveUpdate(ObjectiveInstance objectiveInstance)
		{
			if (EventManager.OnObjectiveUpdate != null)
			{
				EventManager.OnObjectiveUpdate(objectiveInstance.Objective, objectiveInstance.CurrentState);
			}
		}

		public void Call_OnObjectiveSelect(ObjectiveInstance objectiveInstance)
		{
			if (EventManager.OnObjectiveSelect != null)
			{
				EventManager.OnObjectiveSelect(objectiveInstance.Objective, objectiveInstance.CurrentState);
			}
		}

		public void Call_OnPlaySoundtrack(int trackID, bool isMusic, bool loop, float fadeTime, int startingSample)
		{
			if (fadeTime <= 0f)
			{
				fadeTime = 0f;
			}
			if (isMusic)
			{
				if (EventManager.OnPlayMusic != null)
				{
					EventManager.OnPlayMusic(trackID, loop, fadeTime, startingSample);
				}
			}
			else if (EventManager.OnPlayAmbience != null)
			{
				EventManager.OnPlayAmbience(trackID, loop, fadeTime, startingSample);
			}
		}

		public void Call_OnStopSoundtrack(bool isMusic, float fadeTime)
		{
			if (fadeTime <= 0f)
			{
				fadeTime = 0f;
			}
			if (isMusic)
			{
				if (EventManager.OnStopMusic != null)
				{
					EventManager.OnStopMusic(fadeTime);
				}
			}
			else if (EventManager.OnStopAmbience != null)
			{
				EventManager.OnStopAmbience(fadeTime);
			}
		}

		public void Call_OnPlayFootstepSound(Char character, FootstepSounds footstepSounds, bool isWalkingSound, AudioSource audioSource, AudioClip audioClip)
		{
			if (OnPlayFootstepSound != null)
			{
				OnPlayFootstepSound(character, footstepSounds, isWalkingSound, audioSource, audioClip);
			}
		}

		public void Call_OnPlaySound(Sound sound, AudioSource _audioSource, AudioClip audioClip, float fadeInTime)
		{
			if (EventManager.OnPlaySound != null)
			{
				EventManager.OnPlaySound(sound, _audioSource, audioClip, fadeInTime);
			}
		}

		public void Call_OnStopSound(Sound sound, AudioSource _audioSource, AudioClip audioClip, float fadeOutTime)
		{
			if (EventManager.OnStopSound != null)
			{
				EventManager.OnStopSound(sound, _audioSource, audioClip, fadeOutTime);
			}
		}

		public void Call_OnBeginActionList(ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping)
		{
			if (EventManager.OnBeginActionList != null)
			{
				EventManager.OnBeginActionList(actionList, actionListAsset, startingIndex, isSkipping);
			}
		}

		public void Call_OnEndActionList(ActionList actionList, ActionListAsset actionListAsset, bool isSkipping)
		{
			if (EventManager.OnEndActionList != null)
			{
				EventManager.OnEndActionList(actionList, actionListAsset, isSkipping);
			}
		}

		public void Call_OnPauseActionList(ActionList actionList)
		{
			if (EventManager.OnPauseActionList != null)
			{
				EventManager.OnPauseActionList(actionList);
			}
		}

		public void Call_OnResumeActionList(ActionList actionList)
		{
			if (EventManager.OnResumeActionList != null)
			{
				EventManager.OnResumeActionList(actionList);
			}
		}
	}
}
