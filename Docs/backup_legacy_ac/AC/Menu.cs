using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	[Serializable]
	public class Menu : ScriptableObject
	{
		public MenuSource menuSource;

		public Canvas canvas;

		private Canvas runtimeCanvas;

		public int canvasID;

		public RectTransform rectTransform;

		public int rectTransformID;

		public UITransition uiTransitionType;

		public UIPositionType uiPositionType;

		public bool isLocked;

		public int id;

		public string title;

		public Vector2 manualSize = Vector2.zero;

		public AC_PositionType positionType;

		public Vector2 manualPosition = Vector2.zero;

		public bool positionSmoothing;

		public TextAnchor alignment = TextAnchor.MiddleCenter;

		public string toggleKey = string.Empty;

		public bool ignoreMouseClicks;

		public bool pauseWhenEnabled;

		public bool showWhenPaused;

		public bool canClickInCutscene;

		public bool enabledOnStart;

		public ActionListAsset actionListOnTurnOn;

		public ActionListAsset actionListOnTurnOff;

		public bool updateWhenFadeOut = true;

		public bool hideDuringSaveScreenshots = true;

		public bool fitWithinScreen = true;

		public Texture2D backgroundTexture;

		public List<MenuElement> visibleElements = new List<MenuElement>();

		public float transitionProgress;

		public AppearType appearType;

		public SpeechMenuType speechMenuType;

		public SpeechMenuLimit speechMenuLimit;

		public string limitToCharacters = string.Empty;

		public bool forceSubtitles;

		public bool moveWithCharacter = true;

		public MenuElement selected_element;

		public int selected_slot;

		public bool autoSelectFirstVisibleElement;

		public string firstSelectedElement;

		public List<MenuElement> elements = new List<MenuElement>();

		public float spacing;

		public AC_SizeType sizeType;

		public bool autoSizeEveryFrame;

		public MenuOrientation orientation;

		public MenuTransition transitionType = MenuTransition.None;

		public PanDirection panDirection;

		public PanMovement panMovement;

		public AnimationCurve timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		public float panDistance = 0.5f;

		public float fadeSpeed;

		public TextAnchor zoomAnchor = TextAnchor.MiddleCenter;

		public bool zoomElements;

		public bool oneMenuPerSpeech;

		public SpeechProximityLimit speechProximityLimit;

		public float speechProximityDistance = 10f;

		private bool isDuplicate;

		private Vector2 defaultRectTransformLocalPosition = Vector2.zero;

		private bool hasMoved;

		public bool deleteUIWhenTurnOff;

		public Speech speech;

		private InvItem forItem;

		private Hotspot forHotspot;

		private CanvasScaler canvasScaler;

		private CanvasGroup canvasGroup;

		private Animator canvasAnimator;

		private float fadeStartTime;

		private bool isFading;

		private FadeType fadeType;

		private Vector2 panOffset = Vector2.zero;

		private Vector2 dragOffset = Vector2.zero;

		private float zoomAmount = 1f;

		private GameState gameStateWhenTurnedOn;

		private bool isEnabled;

		private bool isDisabledForScreenshot;

		private string idString;

		private bool canDoSmoothing;

		private int elementCount = -1;

		[SerializeField]
		private Vector2 biggestElementSize;

		[SerializeField]
		private Rect rect = default(Rect);

		public InvItem TargetInvItem
		{
			get
			{
				return forItem;
			}
		}

		public Hotspot TargetHotspot
		{
			get
			{
				return forHotspot;
			}
		}

		public string IDString
		{
			get
			{
				return idString;
			}
		}

		public int ID
		{
			set
			{
				id = value;
				idString = id.ToString();
			}
		}

		public bool HasMoved
		{
			get
			{
				return hasMoved;
			}
		}

		public int NumElements
		{
			get
			{
				if (elementCount <= 0)
				{
					elementCount = elements.Count;
				}
				return elementCount;
			}
		}

		public Canvas RuntimeCanvas
		{
			get
			{
				return runtimeCanvas;
			}
		}

		public void Declare(int[] idArray)
		{
			menuSource = MenuSource.AdventureCreator;
			canvas = null;
			runtimeCanvas = null;
			canvasID = 0;
			uiPositionType = UIPositionType.Manual;
			uiTransitionType = UITransition.None;
			spacing = 0.5f;
			orientation = MenuOrientation.Vertical;
			appearType = AppearType.Manual;
			oneMenuPerSpeech = false;
			speechProximityLimit = SpeechProximityLimit.NoLimit;
			speechProximityDistance = 10f;
			moveWithCharacter = true;
			fitWithinScreen = true;
			elements = new List<MenuElement>();
			visibleElements = new List<MenuElement>();
			enabledOnStart = false;
			isEnabled = false;
			sizeType = AC_SizeType.Automatic;
			autoSizeEveryFrame = false;
			speechMenuType = SpeechMenuType.All;
			speechMenuLimit = SpeechMenuLimit.All;
			limitToCharacters = string.Empty;
			forceSubtitles = false;
			actionListOnTurnOn = null;
			actionListOnTurnOff = null;
			firstSelectedElement = string.Empty;
			autoSelectFirstVisibleElement = false;
			fadeSpeed = 0f;
			transitionType = MenuTransition.None;
			panDirection = PanDirection.Up;
			panMovement = PanMovement.Linear;
			timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
			panDistance = 0.5f;
			zoomAnchor = TextAnchor.MiddleCenter;
			zoomElements = false;
			ignoreMouseClicks = false;
			pauseWhenEnabled = false;
			showWhenPaused = false;
			canClickInCutscene = false;
			id = 0;
			isLocked = false;
			updateWhenFadeOut = true;
			hideDuringSaveScreenshots = true;
			positionSmoothing = false;
			defaultRectTransformLocalPosition = Vector2.zero;
			hasMoved = false;
			elementCount = -1;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			title = "Menu " + (id + 1);
		}

		public void CreateDuplicate(Menu menuToCopy)
		{
			Copy(menuToCopy, false);
			LoadUnityUI();
			Recalculate();
			Initalise();
		}

		public void Copy(Menu _menu, bool fromEditor, bool forceUIFields = false)
		{
			menuSource = _menu.menuSource;
			if (forceUIFields || menuSource == MenuSource.UnityUiPrefab || menuSource == MenuSource.UnityUiInScene)
			{
				canvas = _menu.canvas;
				canvasID = _menu.canvasID;
				rectTransform = _menu.rectTransform;
				rectTransformID = _menu.rectTransformID;
			}
			uiTransitionType = _menu.uiTransitionType;
			uiPositionType = _menu.uiPositionType;
			id = _menu.id;
			isLocked = _menu.isLocked;
			title = _menu.title;
			manualSize = _menu.manualSize;
			autoSizeEveryFrame = _menu.autoSizeEveryFrame;
			positionType = _menu.positionType;
			manualPosition = _menu.manualPosition;
			fitWithinScreen = _menu.fitWithinScreen;
			alignment = _menu.alignment;
			toggleKey = _menu.toggleKey;
			backgroundTexture = _menu.backgroundTexture;
			visibleElements = new List<MenuElement>();
			transitionProgress = 0f;
			appearType = _menu.appearType;
			oneMenuPerSpeech = _menu.oneMenuPerSpeech;
			speechProximityLimit = _menu.speechProximityLimit;
			speechProximityDistance = _menu.speechProximityDistance;
			moveWithCharacter = _menu.moveWithCharacter;
			selected_element = null;
			selected_slot = 0;
			firstSelectedElement = _menu.firstSelectedElement;
			autoSelectFirstVisibleElement = _menu.autoSelectFirstVisibleElement;
			spacing = _menu.spacing;
			sizeType = _menu.sizeType;
			orientation = _menu.orientation;
			fadeSpeed = _menu.fadeSpeed;
			transitionType = _menu.transitionType;
			panDirection = _menu.panDirection;
			panMovement = _menu.panMovement;
			timeCurve = _menu.timeCurve;
			panDistance = _menu.panDistance;
			zoomAnchor = _menu.zoomAnchor;
			zoomElements = _menu.zoomElements;
			pauseWhenEnabled = _menu.pauseWhenEnabled;
			showWhenPaused = _menu.showWhenPaused;
			canClickInCutscene = _menu.canClickInCutscene;
			speechMenuType = _menu.speechMenuType;
			speechMenuLimit = _menu.speechMenuLimit;
			enabledOnStart = _menu.enabledOnStart;
			actionListOnTurnOn = _menu.actionListOnTurnOn;
			actionListOnTurnOff = _menu.actionListOnTurnOff;
			ignoreMouseClicks = _menu.ignoreMouseClicks;
			limitToCharacters = _menu.limitToCharacters;
			forceSubtitles = _menu.forceSubtitles;
			updateWhenFadeOut = _menu.updateWhenFadeOut;
			hideDuringSaveScreenshots = _menu.hideDuringSaveScreenshots;
			positionSmoothing = _menu.positionSmoothing;
			idString = id.ToString();
			elementCount = -1;
			elements = new List<MenuElement>();
			bool ignoreUnityUI = Application.isPlaying && !fromEditor && _menu.menuSource == MenuSource.AdventureCreator;
			foreach (MenuElement element in _menu.elements)
			{
				MenuElement item = element.DuplicateSelf(fromEditor, ignoreUnityUI);
				elements.Add(item);
			}
			canDoSmoothing = CanDoSmoothing();
		}

		public void LoadUnityUI()
		{
			if (!IsUnityUI())
			{
				return;
			}
			LocateLocalCanvas();
			EnableUI();
			if (RuntimeCanvas != null)
			{
				rectTransform = Serializer.GetGameObjectComponent<RectTransform>(rectTransformID, RuntimeCanvas.gameObject);
				if (RuntimeCanvas.worldCamera == null)
				{
					RuntimeCanvas.worldCamera = KickStarter.CameraMain;
				}
				if (rectTransform != null && rectTransform.gameObject == RuntimeCanvas.gameObject)
				{
					ACDebug.LogWarning("The menu '" + title + "' uses its Canvas for its RectTransform boundary. The RectTransform boundary should instead be a child object of the Canvas.", RuntimeCanvas.gameObject);
				}
				canvasGroup = RuntimeCanvas.GetComponent<CanvasGroup>();
				canvasScaler = RuntimeCanvas.GetComponent<CanvasScaler>();
				canvasAnimator = RuntimeCanvas.GetComponent<Animator>();
			}
			else
			{
				ACDebug.LogWarning("The Menu '" + title + "' has its Source set to " + menuSource.ToString() + ", but no Linked Canvas can be found!");
			}
			if (IsUnityUI())
			{
				foreach (MenuElement element in elements)
				{
					element.LoadUnityUI(this, RuntimeCanvas);
				}
			}
			if (!isDuplicate)
			{
				DisableUI();
			}
		}

		private void SetAnimState()
		{
			if (!IsUnityUI() || uiTransitionType != UITransition.CustomAnimation || !(fadeSpeed > 0f) || !(RuntimeCanvas != null) || !(canvasAnimator != null) || !RuntimeCanvas.gameObject.activeSelf)
			{
				return;
			}
			if (isFading)
			{
				if (fadeType == FadeType.fadeIn)
				{
					canvasAnimator.Play("On", -1, transitionProgress);
				}
				else
				{
					canvasAnimator.Play("Off", -1, 1f - transitionProgress);
				}
			}
			else if (isEnabled)
			{
				canvasAnimator.Play("OnInstant", -1, 0f);
			}
			else
			{
				canvasAnimator.Play("OffInstant", -1, 0f);
			}
		}

		public bool GetsDuplicated()
		{
			if (oneMenuPerSpeech)
			{
				return appearType == AppearType.WhenSpeechPlays;
			}
			return false;
		}

		public void DuplicateInGame(Menu otherMenu)
		{
			isDuplicate = true;
			Copy(otherMenu, false);
		}

		public void ClearParent()
		{
			if (!GetsDuplicated())
			{
				GameObject gameObject = GameObject.Find("_UI");
				if (gameObject != null && RuntimeCanvas != null && RuntimeCanvas.transform.parent == gameObject.transform)
				{
					RuntimeCanvas.transform.SetParent(null);
				}
			}
		}

		public void Initalise()
		{
			if (appearType == AppearType.Manual && enabledOnStart && !isLocked)
			{
				transitionProgress = 1f;
				EnableUI();
				TurnOn(false);
			}
			else
			{
				transitionProgress = 0f;
				DisableUI();
				TurnOff(false);
			}
			if (transitionType == MenuTransition.Zoom)
			{
				zoomAmount = 0f;
			}
			foreach (MenuElement element in elements)
			{
				element.Initialise(this);
			}
			SetAnimState();
			UpdateTransition();
		}

		public void EnableUI()
		{
			if (menuSource != MenuSource.AdventureCreator && (!GetsDuplicated() || isDuplicate) && RuntimeCanvas != null)
			{
				RuntimeCanvas.gameObject.SetActive(true);
				RuntimeCanvas.enabled = true;
				if (isDuplicate && uiTransitionType == UITransition.CanvasGroupFade && canvasGroup != null && fadeSpeed > 0f)
				{
					canvasGroup.alpha = 0f;
				}
				if (CanCurrentlyKeyboardControl() && IsClickable() && selected_element == null)
				{
					KickStarter.playerMenus.FindFirstSelectedElement();
				}
			}
		}

		public void DisableUI()
		{
			if (RuntimeCanvas != null && menuSource != MenuSource.AdventureCreator)
			{
				isEnabled = false;
				isFading = false;
				if (RuntimeCanvas.gameObject.activeSelf)
				{
					SetAnimState();
					RuntimeCanvas.gameObject.SetActive(false);
				}
				if (KickStarter.playerMenus.DeselectEventSystemMenu(this))
				{
					KickStarter.playerMenus.FindFirstSelectedElement(this);
				}
			}
			if (deleteUIWhenTurnOff)
			{
				if (RuntimeCanvas != null)
				{
					KickStarter.sceneChanger.ScheduleForDeletion(RuntimeCanvas.gameObject);
				}
				KickStarter.playerMenus.UnregisterCustomMenu(this, false);
			}
		}

		public void MakeUIInteractive()
		{
			SetUIInteractableState(true);
		}

		public void MakeUINonInteractive()
		{
			if (!IsClickable())
			{
				SetUIInteractableState(false);
			}
		}

		private void SetUIInteractableState(bool state)
		{
			if (menuSource == MenuSource.AdventureCreator)
			{
				return;
			}
			foreach (MenuElement element in elements)
			{
				element.SetUIInteractableState(state);
			}
		}

		public bool IsUnityUI()
		{
			if (menuSource == MenuSource.UnityUiPrefab || menuSource == MenuSource.UnityUiInScene)
			{
				return true;
			}
			return false;
		}

		public void DrawOutline(MenuElement _selectedElement)
		{
			DrawStraightLine.DrawBox(rect, Color.yellow, 1f, false, 1);
			foreach (MenuElement visibleElement in visibleElements)
			{
				if (visibleElement == _selectedElement)
				{
					visibleElement.DrawOutline(true, this);
				}
				visibleElement.DrawOutline(false, this);
			}
		}

		public void StartDisplay()
		{
			if (isFading)
			{
				GUI.BeginGroup(new Rect(dragOffset.x + panOffset.x + GetRect().x, dragOffset.y + panOffset.y + GetRect().y, GetRect().width * zoomAmount, GetRect().height * zoomAmount));
			}
			else
			{
				GUI.BeginGroup(new Rect(dragOffset.x + GetRect().x, dragOffset.y + GetRect().y, GetRect().width * zoomAmount, GetRect().height * zoomAmount));
			}
			if ((bool)backgroundTexture)
			{
				Rect position = new Rect(0f, 0f, rect.width, rect.height);
				GUI.DrawTexture(position, backgroundTexture, ScaleMode.StretchToFill, true, 0f);
			}
		}

		public void EndDisplay()
		{
			GUI.EndGroup();
		}

		public void SetCentre3D(Vector3 _position)
		{
			if (IsUnityUI())
			{
				if (RuntimeCanvas != null && rectTransform != null && RuntimeCanvas.renderMode == RenderMode.WorldSpace)
				{
					rectTransform.transform.position = _position;
					UpdateDefaultRectTransformLocalPosition();
				}
			}
			else
			{
				SetCentre(new Vector2(_position.x, _position.y));
			}
		}

		public void SetCentre(Vector2 _position, bool useAspectRatio = false)
		{
			if (useAspectRatio && KickStarter.settingsManager != null && !KickStarter.settingsManager.forceAspectRatio)
			{
				useAspectRatio = false;
			}
			if (IsUnityUI())
			{
				if (RuntimeCanvas != null && rectTransform != null)
				{
					if (RuntimeCanvas.renderMode != RenderMode.WorldSpace)
					{
						if (useAspectRatio)
						{
							_position = KickStarter.mainCamera.CorrectScreenPositionForUnityUI(_position);
						}
						Rect safeArea = ACScreen.safeArea;
						if (fitWithinScreen)
						{
							_position -= safeArea.position;
							float num = rectTransform.sizeDelta.x * (1f - rectTransform.pivot.x) * RuntimeCanvas.scaleFactor * rectTransform.localScale.x;
							float num2 = rectTransform.sizeDelta.y * (1f - rectTransform.pivot.y) * RuntimeCanvas.scaleFactor * rectTransform.localScale.y;
							float num3 = rectTransform.sizeDelta.x * rectTransform.pivot.x * RuntimeCanvas.scaleFactor * rectTransform.localScale.x;
							float num4 = rectTransform.sizeDelta.y * rectTransform.pivot.y * RuntimeCanvas.scaleFactor * rectTransform.localScale.y;
							if (KickStarter.settingsManager.forceAspectRatio)
							{
								Vector2 windowViewportDifference = KickStarter.mainCamera.GetWindowViewportDifference();
								num += windowViewportDifference.x;
								num3 += windowViewportDifference.x;
								num2 += windowViewportDifference.y;
								num4 += windowViewportDifference.y;
							}
							_position.x = Mathf.Clamp(_position.x, num3, safeArea.width - num);
							_position.y = Mathf.Clamp(_position.y, num4, safeArea.height - num2);
							_position += safeArea.position;
						}
						if (RuntimeCanvas.renderMode == RenderMode.ScreenSpaceCamera)
						{
							float num5 = 1f;
							if (canvasScaler != null && canvasScaler.enabled && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
							{
								switch (canvasScaler.screenMatchMode)
								{
								case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
								{
									float matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
									num5 = (float)ACScreen.width / canvasScaler.referenceResolution.x * (1f - matchWidthOrHeight) + (float)ACScreen.height / canvasScaler.referenceResolution.y * matchWidthOrHeight;
									break;
								}
								case CanvasScaler.ScreenMatchMode.Expand:
									num5 = Mathf.Min((float)ACScreen.width / canvasScaler.referenceResolution.x, (float)ACScreen.height / canvasScaler.referenceResolution.y);
									break;
								case CanvasScaler.ScreenMatchMode.Shrink:
									num5 = Mathf.Max((float)ACScreen.width / canvasScaler.referenceResolution.x, (float)ACScreen.height / canvasScaler.referenceResolution.y);
									break;
								}
							}
							Vector2 vector = new Vector2(ACScreen.width, ACScreen.height) - safeArea.position - safeArea.position - safeArea.size;
							_position += vector / 2f;
							Vector3 vector2 = new Vector3((_position.x - (float)ACScreen.width / 2f) / num5, (_position.y - (float)ACScreen.height / 2f) / num5, rectTransform.transform.localPosition.z);
							if (canDoSmoothing && !IsFading())
							{
								vector2 = Vector3.Lerp(rectTransform.transform.position, vector2, Time.deltaTime * 12f);
							}
							rectTransform.localPosition = vector2;
							UpdateDefaultRectTransformLocalPosition();
							return;
						}
					}
					Vector3 vector3 = new Vector3(_position.x, _position.y, rectTransform.transform.position.z);
					if (canDoSmoothing && !IsFading())
					{
						vector3 = Vector3.Lerp(rectTransform.transform.position, vector3, Time.deltaTime * 12f);
					}
					rectTransform.transform.position = vector3;
				}
				UpdateDefaultRectTransformLocalPosition();
			}
			else
			{
				Vector2 zero = Vector2.zero;
				if (useAspectRatio)
				{
					Vector2 vector4 = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
					Vector2 vector5 = ((!(KickStarter.mainCamera != null)) ? Vector2.zero : KickStarter.mainCamera.GetMainGameViewOffset());
					Vector2 vector6 = new Vector2(_position.x * vector4.x + vector5.x, _position.y * vector4.y + vector5.y);
					zero = new Vector2(vector6.x - rect.width / 2f, vector6.y - rect.height / 2f);
				}
				else
				{
					Vector2 size = ACScreen.safeArea.size;
					Vector2 vector7 = new Vector2(_position.x * size.x, _position.y * size.y);
					zero = new Vector2(vector7.x - rect.width / 2f, vector7.y - rect.height / 2f);
					zero += new Vector2(ACScreen.safeArea.x, (float)ACScreen.height - ACScreen.safeArea.height - ACScreen.safeArea.y);
				}
				rect.position = zero;
				FitMenuInsideScreen();
				UpdateDefaultRectTransformLocalPosition();
			}
		}

		private bool CanDoSmoothing(bool forGUI = false)
		{
			if ((!CanPause() || !pauseWhenEnabled) && (forGUI || (positionSmoothing && Application.isPlaying)) && menuSource == MenuSource.UnityUiPrefab && (uiPositionType == UIPositionType.AbovePlayer || uiPositionType == UIPositionType.AboveSpeakingCharacter || uiPositionType == UIPositionType.FollowCursor))
			{
				return true;
			}
			return false;
		}

		private Vector2 GetCentre()
		{
			return new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
		}

		private void FitMenuInsideScreen()
		{
			if ((positionType != AC_PositionType.Manual && positionType != AC_PositionType.FollowCursor && positionType != AC_PositionType.AppearAtCursorAndFreeze && positionType != AC_PositionType.OnHotspot && positionType != AC_PositionType.AboveSpeakingCharacter && positionType != AC_PositionType.AbovePlayer) || !fitWithinScreen)
			{
				return;
			}
			Vector2 vector = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
			Vector2 vector2 = ((!(KickStarter.mainCamera != null)) ? Vector2.zero : KickStarter.mainCamera.GetMainGameViewOffset());
			if (rect.x < vector2.x)
			{
				rect.x = vector2.x;
			}
			else
			{
				float num = vector.x + vector2.x - rect.width;
				if (rect.x > num)
				{
					rect.x = num;
				}
			}
			if (rect.y < vector2.y)
			{
				rect.y = vector2.y;
				return;
			}
			float num2 = vector.y + vector2.y - rect.height;
			if (rect.y > num2)
			{
				rect.y = num2;
			}
		}

		public void Align(TextAnchor _anchor)
		{
			Vector2 vector = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
			Vector2 vector2 = ((!(KickStarter.mainCamera != null)) ? Vector2.zero : KickStarter.mainCamera.GetMainGameViewOffset());
			switch (_anchor)
			{
			case TextAnchor.UpperLeft:
			case TextAnchor.MiddleLeft:
			case TextAnchor.LowerLeft:
				rect.x = vector2.x;
				break;
			case TextAnchor.UpperCenter:
			case TextAnchor.MiddleCenter:
			case TextAnchor.LowerCenter:
				rect.x = (vector.x - rect.width) / 2f + vector2.x;
				break;
			default:
				rect.x = vector.x - rect.width + vector2.x;
				break;
			}
			switch (_anchor)
			{
			case TextAnchor.LowerLeft:
			case TextAnchor.LowerCenter:
			case TextAnchor.LowerRight:
				rect.y = vector.y - rect.height + vector2.y;
				break;
			case TextAnchor.MiddleLeft:
			case TextAnchor.MiddleCenter:
			case TextAnchor.MiddleRight:
				rect.y = (vector.y - rect.height) / 2f + vector2.y;
				break;
			default:
				rect.y = vector2.y;
				break;
			}
		}

		private void SetManualSize(Vector2 _size)
		{
			Vector2 vector = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
			rect.width = _size.x * vector.x;
			rect.height = _size.y * vector.y;
		}

		public bool IsPointInside(Vector2 _point)
		{
			if (menuSource == MenuSource.AdventureCreator)
			{
				return GetRect().Contains(_point);
			}
			if (rectTransform != null && RuntimeCanvas != null)
			{
				if (ignoreMouseClicks && canvasGroup != null && !canvasGroup.interactable)
				{
					return false;
				}
				bool flag = false;
				bool flag2 = false;
				if (!RuntimeCanvas.gameObject.activeSelf)
				{
					RuntimeCanvas.gameObject.SetActive(true);
					flag = true;
				}
				flag2 = ((RuntimeCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? RectTransformUtility.RectangleContainsScreenPoint(rectTransform, new Vector2(_point.x, (float)ACScreen.height - _point.y), RuntimeCanvas.worldCamera) : RectTransformUtility.RectangleContainsScreenPoint(rectTransform, new Vector2(_point.x, (float)ACScreen.height - _point.y), null));
				if (flag)
				{
					RuntimeCanvas.gameObject.SetActive(false);
				}
				return flag2;
			}
			return false;
		}

		public Rect GetRect()
		{
			if (!Application.isPlaying)
			{
				if ((bool)KickStarter.mainCamera)
				{
					return KickStarter.mainCamera.LimitMenuToAspect(rect);
				}
				return rect;
			}
			return rect;
		}

		public bool IsPointerOverSlot(MenuElement _element, int slot, Vector2 _point)
		{
			if (menuSource == MenuSource.AdventureCreator)
			{
				Rect slotRectRelative = _element.GetSlotRectRelative(slot);
				return GetRectAbsolute(slotRectRelative).Contains(_point);
			}
			if (RuntimeCanvas != null)
			{
				if (ignoreMouseClicks && canvasGroup != null && !canvasGroup.interactable)
				{
					return false;
				}
				RectTransform rectTransform = _element.GetRectTransform(slot);
				if (rectTransform != null)
				{
					if (RuntimeCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, new Vector2(_point.x, (float)ACScreen.height - _point.y), null);
					}
					return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, new Vector2(_point.x, (float)ACScreen.height - _point.y), RuntimeCanvas.worldCamera);
				}
			}
			return false;
		}

		public Rect GetRectAbsolute(Rect _rectRelative)
		{
			return new Rect(_rectRelative.x + dragOffset.x + GetRect().x, _rectRelative.y + dragOffset.y + GetRect().y, _rectRelative.width, _rectRelative.height);
		}

		public void ResetVisibleElements()
		{
			visibleElements.Clear();
			foreach (MenuElement element in elements)
			{
				element.RecalculateSize(menuSource);
				if (element.IsVisible)
				{
					visibleElements.Add(element);
				}
			}
		}

		public void RefreshDialogueOptions()
		{
			bool flag = false;
			if (!IsOff())
			{
				foreach (MenuElement visibleElement in visibleElements)
				{
					if (visibleElement is MenuDialogList)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				Recalculate();
			}
		}

		public void Recalculate()
		{
			if (IsUnityUI())
			{
				AutoResize();
				return;
			}
			ResetVisibleElements();
			PositionElements();
			if (sizeType == AC_SizeType.Automatic)
			{
				AutoResize();
			}
			else if (sizeType == AC_SizeType.Manual)
			{
				SetManualSize(new Vector2(manualSize.x / 100f, manualSize.y / 100f));
			}
			else if (sizeType == AC_SizeType.AbsolutePixels)
			{
				rect.width = manualSize.x;
				rect.height = manualSize.y;
			}
			if (positionType == AC_PositionType.Centred)
			{
				Centre();
				manualPosition = GetCentre();
			}
			else if (positionType == AC_PositionType.Aligned)
			{
				Align(alignment);
				manualPosition = GetCentre();
			}
			else if (positionType == AC_PositionType.Manual || !Application.isPlaying)
			{
				SetCentre(new Vector2(manualPosition.x / 100f, manualPosition.y / 100f), true);
			}
		}

		public void AutoResize(MenuElement elementToSkip = null)
		{
			visibleElements.Clear();
			biggestElementSize = default(Vector2);
			foreach (MenuElement element in elements)
			{
				if (!(element != null))
				{
					continue;
				}
				if (elementToSkip == null || element != elementToSkip)
				{
					element.RecalculateSize(menuSource);
				}
				if (!element.IsVisible)
				{
					continue;
				}
				visibleElements.Add(element);
				if (menuSource == MenuSource.AdventureCreator)
				{
					if (element.GetSizeFromCorner().x > biggestElementSize.x)
					{
						biggestElementSize.x = element.GetSizeFromCorner().x;
					}
					if (element.GetSizeFromCorner().y > biggestElementSize.y)
					{
						biggestElementSize.y = element.GetSizeFromCorner().y;
					}
				}
			}
			if (menuSource == MenuSource.AdventureCreator)
			{
				Vector2 vector = ((!(KickStarter.mainCamera != null)) ? new Vector2(ACScreen.width, ACScreen.height) : KickStarter.mainCamera.GetPlayableScreenArea(false).size);
				rect.width = spacing / 100f * vector.x + biggestElementSize.x;
				rect.height = spacing / 100f * vector.x + biggestElementSize.y;
				manualSize = new Vector2(rect.width * 100f / vector.x, rect.height * 100f / vector.y);
			}
		}

		private void PositionElements()
		{
			float num = 0f;
			foreach (MenuElement visibleElement in visibleElements)
			{
				if (menuSource != MenuSource.AdventureCreator)
				{
					visibleElement.RecalculateSize(menuSource);
					break;
				}
				if (visibleElement == null)
				{
					ACDebug.Log("Null element found");
					break;
				}
				if (visibleElement.positionType == AC_PositionType2.RelativeToMenuSize && sizeType == AC_SizeType.Automatic)
				{
					ACDebug.LogError("Menu " + title + " cannot display because its size is Automatic, while its Element " + visibleElement.title + "'s Position is set to Relative");
					break;
				}
				if (visibleElement.positionType == AC_PositionType2.RelativeToMenuSize)
				{
					visibleElement.SetRelativePosition(new Vector2(rect.width / 100f, rect.height / 100f));
				}
				else if (orientation == MenuOrientation.Horizontal)
				{
					float num2 = ((!(KickStarter.mainCamera != null)) ? ((float)ACScreen.width) : KickStarter.mainCamera.GetPlayableScreenArea(false).size.x);
					if (visibleElement.positionType == AC_PositionType2.Aligned)
					{
						visibleElement.SetPosition(new Vector2(spacing / 100f * num2 + num, spacing / 100f * num2));
					}
					num += visibleElement.GetSize().x + spacing / 100f * num2;
				}
				else
				{
					float num3 = ((!(KickStarter.mainCamera != null)) ? ((float)ACScreen.width) : KickStarter.mainCamera.GetPlayableScreenArea(false).size.x);
					if (visibleElement.positionType == AC_PositionType2.Aligned)
					{
						visibleElement.SetPosition(new Vector2(spacing / 100f * num3, spacing / 100f * num3 + num));
					}
					num += visibleElement.GetSize().y + spacing / 100f * num3;
				}
			}
		}

		public void Centre()
		{
			SetCentre(new Vector2(0.5f, 0.5f));
		}

		public bool IsEnabled()
		{
			if (isLocked)
			{
				if (isFading && fadeType == FadeType.fadeOut)
				{
					return isEnabled;
				}
				return false;
			}
			return isEnabled;
		}

		public bool IsVisible()
		{
			if (transitionProgress >= 1f && isEnabled)
			{
				return true;
			}
			return false;
		}

		private void EndTransitionOn()
		{
			transitionProgress = 1f;
			isEnabled = true;
			isFading = false;
		}

		private void EndTransitionOff()
		{
			transitionProgress = 0f;
			isFading = false;
			isEnabled = false;
			SetAnimState();
			ReturnGameState();
			DisableUI();
			ClearSpeechText();
			KickStarter.playerMenus.CheckCrossfade(this);
		}

		public bool IsOn()
		{
			if (!isLocked && isEnabled && !isFading)
			{
				return true;
			}
			return false;
		}

		public bool IsOff()
		{
			if (isLocked)
			{
				return true;
			}
			if (!isEnabled)
			{
				return true;
			}
			return false;
		}

		public bool HasTransition()
		{
			if (fadeSpeed <= 0f)
			{
				return false;
			}
			if (IsUnityUI())
			{
				if (uiTransitionType != UITransition.None)
				{
					return true;
				}
			}
			else if (transitionType != MenuTransition.None)
			{
				return true;
			}
			return false;
		}

		public GameState GetGameStateWhenTurnedOn()
		{
			return gameStateWhenTurnedOn;
		}

		public bool IsElementSelectedByEventSystem(int elementIndex, int slotIndex)
		{
			if (menuSource != MenuSource.AdventureCreator)
			{
				return elements[elementIndex].IsSelectedByEventSystem(slotIndex);
			}
			return false;
		}

		private void InitUIElements()
		{
			if (!IsUnityUI())
			{
				return;
			}
			int language = Options.GetLanguage();
			for (int i = 0; i < NumElements; i++)
			{
				if (elements[i].GetNumSlots() == 0 || !elements[i].IsVisible)
				{
					elements[i].HideAllUISlots();
				}
				for (int j = 0; j < elements[i].GetNumSlots(); j++)
				{
					elements[i].PreDisplay(j, language, false);
				}
			}
		}

		public bool TurnOn(bool doFade = true)
		{
			if (IsOn())
			{
				return false;
			}
			gameStateWhenTurnedOn = KickStarter.stateHandler.gameState;
			KickStarter.playerMenus.UpdateMenuPosition(this, KickStarter.playerInput.GetInvertedMouse(), true);
			if (!HasTransition())
			{
				doFade = false;
			}
			if (!isLocked && (!isEnabled || (isFading && fadeType == FadeType.fadeOut)))
			{
				selected_slot = 0;
				selected_element = null;
				if ((bool)KickStarter.playerInput)
				{
					if (menuSource == MenuSource.AdventureCreator && positionType == AC_PositionType.AppearAtCursorAndFreeze)
					{
						Vector2 vector = KickStarter.mainCamera.ConvertToMenuSpace(KickStarter.playerInput.GetInvertedMouse());
						SetCentre(new Vector2(vector.x + (manualPosition.x - 50f) / 100f, vector.y + (manualPosition.y - 50f) / 100f));
					}
					else if (menuSource != MenuSource.AdventureCreator && uiPositionType == UIPositionType.AppearAtCursorAndFreeze)
					{
						EnableUI();
						SetCentre(new Vector2(KickStarter.playerInput.GetInvertedMouse().x, (float)ACScreen.height + 1f - KickStarter.playerInput.GetInvertedMouse().y));
					}
				}
				MenuSystem.OnMenuEnable(this);
				foreach (MenuElement element in elements)
				{
					element.OnMenuTurnOn(this);
				}
				ChangeGameState();
				Recalculate();
				InitUIElements();
				dragOffset = Vector2.zero;
				isEnabled = true;
				isFading = doFade;
				if (actionListOnTurnOn != null)
				{
					AdvGame.RunActionListAsset(actionListOnTurnOn);
				}
				EnableUI();
				KickStarter.eventManager.Call_OnMenuTurnOn(this, !doFade);
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeIn;
					fadeStartTime = Time.realtimeSinceStartup - transitionProgress * fadeSpeed;
				}
				else
				{
					transitionProgress = 1f;
					isEnabled = true;
					isFading = false;
					if (IsUnityUI())
					{
						UpdateTransition();
					}
				}
				SetAnimState();
			}
			return true;
		}

		public bool TurnOff(bool doFade = true)
		{
			if (IsOff())
			{
				return false;
			}
			bool flag = !IsFadingOut();
			if (appearType == AppearType.OnContainer)
			{
				KickStarter.playerInput.activeContainer = null;
			}
			else if (appearType == AppearType.OnViewDocument)
			{
				KickStarter.runtimeDocuments.CloseDocument();
			}
			if (!HasTransition())
			{
				doFade = false;
			}
			if (flag || !doFade)
			{
				KickStarter.eventManager.Call_OnMenuTurnOff(this, !doFade);
			}
			if (isEnabled && (!isFading || (isFading && fadeType == FadeType.fadeIn)))
			{
				isFading = doFade;
				if (doFade && fadeSpeed > 0f)
				{
					fadeType = FadeType.fadeOut;
					fadeStartTime = Time.realtimeSinceStartup - (1f - transitionProgress) * fadeSpeed;
					SetAnimState();
				}
				else
				{
					transitionProgress = 0f;
					UpdateTransition();
					isFading = false;
					isEnabled = false;
					ReturnGameState();
					DisableUI();
					ClearSpeechText();
				}
			}
			if (flag)
			{
				if (actionListOnTurnOff != null)
				{
					AdvGame.RunActionListAsset(actionListOnTurnOff);
				}
				KickStarter.playerMenus.OnTurnOffMenu(this);
			}
			return true;
		}

		public void ForceOff(bool ignoreActionList = false)
		{
			if (isEnabled || isFading)
			{
				if (!ignoreActionList && actionListOnTurnOff != null && !IsFadingOut())
				{
					AdvGame.RunActionListAsset(actionListOnTurnOff);
				}
				KickStarter.eventManager.Call_OnMenuTurnOff(this, true);
				transitionProgress = 0f;
				UpdateTransition();
				isFading = false;
				isEnabled = false;
				DisableUI();
				ClearSpeechText();
				ReturnGameState();
			}
		}

		public bool IsFadingIn()
		{
			if (isFading && fadeType == FadeType.fadeIn)
			{
				return true;
			}
			return false;
		}

		public bool IsFadingOut()
		{
			if (isFading && fadeType == FadeType.fadeOut)
			{
				return true;
			}
			return false;
		}

		public bool IsFading()
		{
			return isFading;
		}

		public float GetFadeProgress()
		{
			if (panMovement == PanMovement.Linear)
			{
				return 1f - transitionProgress;
			}
			if (panMovement == PanMovement.Smooth)
			{
				return transitionProgress * transitionProgress - 2f * transitionProgress + 1f;
			}
			if (panMovement == PanMovement.CustomCurve)
			{
				float time = timeCurve[0].time;
				float time2 = timeCurve[timeCurve.length - 1].time;
				return 1f - timeCurve.Evaluate((time2 - time) * transitionProgress);
			}
			return 0f;
		}

		public void HandleTransition()
		{
			if (!isFading || !isEnabled)
			{
				return;
			}
			if (fadeType == FadeType.fadeIn)
			{
				transitionProgress = (Time.realtimeSinceStartup - fadeStartTime) / fadeSpeed;
				if (transitionProgress > 1f)
				{
					transitionProgress = 1f;
					UpdateTransition();
					EndTransitionOn();
				}
				else
				{
					UpdateTransition();
				}
			}
			else
			{
				transitionProgress = 1f - (Time.realtimeSinceStartup - fadeStartTime) / fadeSpeed;
				if (transitionProgress < 0f)
				{
					transitionProgress = 0f;
					UpdateTransition();
					EndTransitionOff();
				}
				else
				{
					UpdateTransition();
				}
			}
		}

		private void UpdateTransition()
		{
			if (IsUnityUI())
			{
				if (uiTransitionType == UITransition.CanvasGroupFade && canvasGroup != null && fadeSpeed > 0f)
				{
					canvasGroup.alpha = 1f - GetFadeProgress();
				}
			}
			else
			{
				if (transitionType == MenuTransition.Fade)
				{
					return;
				}
				if (transitionType == MenuTransition.FadeAndPan || transitionType == MenuTransition.Pan)
				{
					float num = GetFadeProgress() * panDistance;
					if (panDirection == PanDirection.Down)
					{
						panOffset = new Vector2(0f, num);
					}
					else if (panDirection == PanDirection.Left)
					{
						panOffset = new Vector2(0f - num, 0f);
					}
					else if (panDirection == PanDirection.Up)
					{
						panOffset = new Vector2(0f, 0f - num);
					}
					else if (panDirection == PanDirection.Right)
					{
						panOffset = new Vector2(num, 0f);
					}
					panOffset = new Vector2(panOffset.x * KickStarter.mainCamera.GetPlayableScreenArea(false).size.x, panOffset.y * KickStarter.mainCamera.GetPlayableScreenArea(false).size.y);
				}
				else if (transitionType == MenuTransition.Zoom)
				{
					zoomAmount = 1f - GetFadeProgress();
					if (zoomAnchor == TextAnchor.UpperLeft)
					{
						panOffset = Vector2.zero;
					}
					else if (zoomAnchor == TextAnchor.UpperCenter)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width / 2f, 0f);
					}
					else if (zoomAnchor == TextAnchor.UpperRight)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width, 0f);
					}
					else if (zoomAnchor == TextAnchor.MiddleLeft)
					{
						panOffset = new Vector2(0f, (1f - zoomAmount) * rect.height / 2f);
					}
					else if (zoomAnchor == TextAnchor.MiddleCenter)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height / 2f);
					}
					else if (zoomAnchor == TextAnchor.MiddleRight)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height / 2f);
					}
					else if (zoomAnchor == TextAnchor.LowerLeft)
					{
						panOffset = new Vector2(0f, (1f - zoomAmount) * rect.height);
					}
					else if (zoomAnchor == TextAnchor.LowerCenter)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width / 2f, (1f - zoomAmount) * rect.height);
					}
					else if (zoomAnchor == TextAnchor.LowerRight)
					{
						panOffset = new Vector2((1f - zoomAmount) * rect.width, (1f - zoomAmount) * rect.height);
					}
				}
			}
		}

		public void AfterSceneChange()
		{
			if (menuSource == MenuSource.UnityUiInScene)
			{
				LoadUnityUI();
				Initalise();
			}
			else if (menuSource == MenuSource.UnityUiPrefab && RuntimeCanvas != null && RuntimeCanvas.worldCamera == null)
			{
				RuntimeCanvas.worldCamera = KickStarter.CameraMain;
			}
			if (IsOn())
			{
				ChangeGameState();
			}
		}

		private void ChangeGameState()
		{
			if (IsBlocking() && Application.isPlaying)
			{
				if (appearType != AppearType.OnInteraction)
				{
					KickStarter.playerInteraction.DeselectHotspot(true);
				}
				KickStarter.mainCamera.FadeIn(0f);
				KickStarter.mainCamera.PauseGame(true);
			}
		}

		private void ReturnGameState()
		{
			if (IsBlocking() && !KickStarter.playerMenus.ArePauseMenusOn(this) && Application.isPlaying)
			{
				KickStarter.stateHandler.RestoreLastNonPausedState();
			}
		}

		public bool CanPause()
		{
			switch (appearType)
			{
			case AppearType.Manual:
			case AppearType.MouseOver:
			case AppearType.OnInputKey:
			case AppearType.OnInteraction:
			case AppearType.OnContainer:
			case AppearType.OnViewDocument:
				return true;
			default:
				return false;
			}
		}

		public bool IsClickable()
		{
			if (ignoreMouseClicks)
			{
				return false;
			}
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				if (canClickInCutscene && ShowClickInCutscenesOption())
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public bool CanClickInCutscenes()
		{
			if (ShowClickInCutscenesOption() && !ignoreMouseClicks && canClickInCutscene)
			{
				return true;
			}
			return false;
		}

		private bool ShowClickInCutscenesOption()
		{
			if (appearType == AppearType.WhenSpeechPlays || appearType == AppearType.DuringConversation || appearType == AppearType.Manual || appearType == AppearType.WhenSpeechPlays || appearType == AppearType.DuringCutscene)
			{
				return true;
			}
			return false;
		}

		public bool IsBlocking()
		{
			if (pauseWhenEnabled && CanPause())
			{
				return true;
			}
			return false;
		}

		public bool IsManualControlled()
		{
			if (appearType == AppearType.Manual || appearType == AppearType.OnInputKey || appearType == AppearType.OnContainer || appearType == AppearType.OnViewDocument)
			{
				return true;
			}
			return false;
		}

		public void MatchInteractions(Hotspot hotspot, bool includeInventory)
		{
			forHotspot = hotspot;
			forItem = null;
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					if (KickStarter.settingsManager.autoHideInteractionIcons)
					{
						MenuInteraction menuInteraction = (MenuInteraction)element;
						menuInteraction.MatchInteractions(hotspot.useButtons);
					}
				}
				else if (element is MenuInventoryBox)
				{
					if (includeInventory)
					{
						element.RecalculateSize(menuSource);
						Recalculate();
						element.AutoSetVisibility();
					}
					else
					{
						element.IsVisible = false;
					}
				}
			}
			Recalculate();
			Recalculate();
		}

		public void MatchInteractions(InvItem item, bool includeInventory)
		{
			forHotspot = null;
			forItem = item;
			foreach (MenuElement element in elements)
			{
				if (KickStarter.settingsManager.autoHideInteractionIcons && element is MenuInteraction)
				{
					MenuInteraction menuInteraction = (MenuInteraction)element;
					menuInteraction.MatchInteractions(item);
				}
				else if (element is MenuInventoryBox)
				{
					if (includeInventory)
					{
						element.RecalculateSize(menuSource);
						Recalculate();
						element.AutoSetVisibility();
					}
					else
					{
						element.IsVisible = false;
					}
				}
			}
			Recalculate();
			Recalculate();
		}

		public void MatchLookInteraction()
		{
			foreach (MenuElement element in elements)
			{
				MenuInteraction menuInteraction = element as MenuInteraction;
				if (menuInteraction != null)
				{
					menuInteraction.MatchInteraction(KickStarter.cursorManager.lookCursor_ID);
				}
			}
		}

		public void MatchUseInteraction(Button button)
		{
			foreach (MenuElement element in elements)
			{
				MenuInteraction menuInteraction = element as MenuInteraction;
				if (menuInteraction != null)
				{
					menuInteraction.MatchUseInteraction(button);
				}
			}
		}

		public void HideInteractions()
		{
			foreach (MenuElement element in elements)
			{
				if (element is MenuInteraction)
				{
					element.IsVisible = false;
					element.isClickable = false;
				}
			}
		}

		public void SetDragOffset(Vector2 pos, Rect dragRect)
		{
			if (pos.x < dragRect.x)
			{
				pos.x = dragRect.x;
			}
			else if (pos.x > dragRect.x + dragRect.width - GetRect().width)
			{
				pos.x = dragRect.x + dragRect.width - GetRect().width;
			}
			if (pos.y < dragRect.y)
			{
				pos.y = dragRect.y;
			}
			else if (pos.y > dragRect.y + dragRect.height - GetRect().height)
			{
				pos.y = dragRect.y + dragRect.height - GetRect().height;
			}
			dragOffset = pos;
		}

		public Vector2 GetDragStart()
		{
			return dragOffset;
		}

		public float GetZoom()
		{
			if (!IsUnityUI() && transitionType == MenuTransition.Zoom && zoomElements)
			{
				return zoomAmount;
			}
			return 1f;
		}

		public bool CanCurrentlyKeyboardControl()
		{
			if (ignoreMouseClicks)
			{
				return false;
			}
			if ((menuSource != MenuSource.AdventureCreator || KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen) && ((KickStarter.stateHandler.gameState == GameState.Paused && IsBlocking() && KickStarter.menuManager.keyboardControlWhenPaused) || (KickStarter.stateHandler.gameState == GameState.DialogOptions && appearType == AppearType.DuringConversation && KickStarter.menuManager.keyboardControlWhenDialogOptions) || (KickStarter.stateHandler.gameState == GameState.Cutscene && CanClickInCutscenes()) || (KickStarter.stateHandler.IsInGameplay() && KickStarter.playerInput.canKeyboardControlMenusDuringGameplay && CanPause() && !pauseWhenEnabled)))
			{
				return true;
			}
			return false;
		}

		public void Select(string elementName, int slotIndex = 0)
		{
			MenuElement elementWithName = GetElementWithName(elementName);
			if (elementWithName != null)
			{
				if (elementWithName.IsVisible)
				{
					selected_element = elementWithName;
					selected_slot = slotIndex;
					if (IsUnityUI() && IsEnabled())
					{
						GameObject objectToSelect = selected_element.GetObjectToSelect(selected_slot);
						if (objectToSelect != null)
						{
							KickStarter.playerMenus.SelectUIElement(objectToSelect);
						}
					}
				}
				else
				{
					ACDebug.LogWarning("Cannot select element '" + elementName + "' inside Menu '" + title + "' because it is not visible!");
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot find element '" + elementName + "' inside Menu '" + title + "'");
			}
		}

		public void AutoSelect()
		{
			if (visibleElements == null || visibleElements.Count == 0 || menuSource != MenuSource.AdventureCreator)
			{
				return;
			}
			if (selected_element != null)
			{
				if (!selected_element.IsVisible)
				{
					GetNearestSlot(selected_element, selected_slot);
				}
				return;
			}
			for (int i = 0; i < visibleElements.Count; i++)
			{
				if (visibleElements[i].isClickable)
				{
					selected_element = visibleElements[i];
					break;
				}
			}
		}

		public bool GetNextSlot(Vector2 inputDirection, bool scrollingLocked)
		{
			if (menuSource != MenuSource.AdventureCreator)
			{
				return false;
			}
			if (inputDirection.y > 0.1f)
			{
				GetNextSlot(Vector2.down, scrollingLocked, selected_element, selected_slot);
				return true;
			}
			if (inputDirection.y < -0.1f)
			{
				GetNextSlot(Vector2.up, scrollingLocked, selected_element, selected_slot);
				return true;
			}
			if (inputDirection.x < -0.1f)
			{
				GetNextSlot(Vector2.left, scrollingLocked, selected_element, selected_slot);
				return true;
			}
			if (inputDirection.x > 0.1f)
			{
				GetNextSlot(Vector2.right, scrollingLocked, selected_element, selected_slot);
				return true;
			}
			return false;
		}

		private void GetNextSlot(Vector2 direction, bool scrollingLocked, MenuElement currentElement, int currentSlotIndex)
		{
			if (currentElement == null)
			{
				return;
			}
			if (currentElement is MenuSlider)
			{
				MenuSlider menuSlider = currentElement as MenuSlider;
				if (menuSlider.KeyboardControl(direction))
				{
					return;
				}
			}
			if (scrollingLocked)
			{
				return;
			}
			Vector2 center = GetRectAbsolute(currentElement.GetSlotRectRelative(currentSlotIndex)).center;
			MenuElement menuElement = currentElement;
			int num = currentSlotIndex;
			float num2 = -1f;
			foreach (MenuElement visibleElement in visibleElements)
			{
				Vector2[] slotCentres = visibleElement.GetSlotCentres(this);
				if (slotCentres == null)
				{
					continue;
				}
				for (int i = 0; i < slotCentres.Length; i++)
				{
					Vector2 lhs = slotCentres[i] - center;
					float num3 = Vector2.Dot(lhs, direction);
					Vector2 rhs = Quaternion.Euler(0f, 0f, 90f) * direction;
					float num4 = Vector2.Dot(lhs, rhs);
					float num5 = num3 / lhs.sqrMagnitude;
					if (num3 > 0f && Mathf.Abs(num3) > Mathf.Abs(num4 / 2f))
					{
						float sqrMagnitude = lhs.sqrMagnitude;
						if (!Mathf.Approximately(sqrMagnitude, 0f) && (num5 > num2 || num2 < 0f))
						{
							menuElement = visibleElement;
							num = i;
							num2 = num5;
						}
					}
				}
			}
			selected_slot = num;
			selected_element = menuElement;
		}

		private void GetNearestSlot(MenuElement currentElement, int currentSlotIndex)
		{
			if (currentElement == null)
			{
				return;
			}
			Vector2 center = GetRectAbsolute(currentElement.GetSlotRectRelative(currentSlotIndex)).center;
			MenuElement menuElement = currentElement;
			int num = currentSlotIndex;
			float num2 = -1f;
			foreach (MenuElement visibleElement in visibleElements)
			{
				Vector2[] slotCentres = visibleElement.GetSlotCentres(this);
				if (slotCentres == null)
				{
					continue;
				}
				for (int i = 0; i < slotCentres.Length; i++)
				{
					float sqrMagnitude = (slotCentres[i] - center).sqrMagnitude;
					if (sqrMagnitude < num2 || num2 < 0f)
					{
						menuElement = visibleElement;
						num = i;
						num2 = sqrMagnitude;
					}
				}
			}
			selected_slot = num;
			selected_element = menuElement;
		}

		public MenuElement GetElementWithName(string menuElementName)
		{
			foreach (MenuElement element in elements)
			{
				if (element.title == menuElementName)
				{
					return element;
				}
			}
			return null;
		}

		public Vector2 GetSlotCentre(MenuElement _element, int slot)
		{
			foreach (MenuElement element in elements)
			{
				if (!(element == _element))
				{
					continue;
				}
				if (IsUnityUI())
				{
					Vector3 position = element.GetRectTransform(slot).position;
					if (RuntimeCanvas.renderMode != RenderMode.WorldSpace)
					{
						return new Vector2(position.x, (float)ACScreen.height - position.y);
					}
					return KickStarter.CameraMain.WorldToScreenPoint(position);
				}
				Rect slotRectRelative = _element.GetSlotRectRelative(slot);
				return new Vector2(GetRect().x + slotRectRelative.x + slotRectRelative.width / 2f, GetRect().y + slotRectRelative.y + slotRectRelative.height / 2f);
			}
			return Vector2.zero;
		}

		private void ClearSpeechText()
		{
			foreach (MenuElement element in elements)
			{
				element.ClearSpeech();
			}
		}

		public void SetHotspot(Hotspot _hotspot, InvItem _invItem)
		{
			forHotspot = _hotspot;
			forItem = _invItem;
		}

		public void SetSpeech(Speech _speech)
		{
			speech = _speech;
			foreach (MenuElement element in elements)
			{
				element.SetSpeech(_speech);
			}
		}

		public GameObject GetObjectToSelect()
		{
			if (autoSelectFirstVisibleElement)
			{
				foreach (MenuElement visibleElement in visibleElements)
				{
					if (visibleElement.IsVisible)
					{
						GameObject objectToSelect = visibleElement.GetObjectToSelect();
						if (objectToSelect != null)
						{
							return objectToSelect;
						}
					}
				}
			}
			else
			{
				if (string.IsNullOrEmpty(firstSelectedElement))
				{
					return null;
				}
				foreach (MenuElement visibleElement2 in visibleElements)
				{
					if (visibleElement2.title == firstSelectedElement)
					{
						return visibleElement2.GetObjectToSelect();
					}
				}
			}
			return null;
		}

		public void PreScreenshotBackup()
		{
			if (menuSource != MenuSource.AdventureCreator && RuntimeCanvas != null)
			{
				isDisabledForScreenshot = hideDuringSaveScreenshots && RuntimeCanvas.gameObject.activeSelf;
				if (isDisabledForScreenshot)
				{
					RuntimeCanvas.gameObject.SetActive(false);
				}
			}
		}

		public void PostScreenshotBackup()
		{
			if (menuSource != MenuSource.AdventureCreator && RuntimeCanvas != null && isDisabledForScreenshot)
			{
				RuntimeCanvas.gameObject.SetActive(true);
			}
		}

		public bool ShouldTurnOffWhenLoading()
		{
			if (IsManualControlled())
			{
				foreach (MenuElement element in elements)
				{
					if (element is MenuSavesList)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void UpdateDefaultRectTransformLocalPosition()
		{
			if (IsUnityUI() && !hasMoved && defaultRectTransformLocalPosition == Vector2.zero && rectTransform != null)
			{
				defaultRectTransformLocalPosition = rectTransform.localPosition;
				if (defaultRectTransformLocalPosition != Vector2.zero)
				{
					hasMoved = true;
				}
			}
		}

		private void LocateLocalCanvas()
		{
			Canvas canvas = null;
			if (menuSource == MenuSource.UnityUiPrefab)
			{
				if (this.canvas != null)
				{
					canvas = UnityEngine.Object.Instantiate(this.canvas);
					canvas.gameObject.name = this.canvas.name;
					UnityEngine.Object.DontDestroyOnLoad(canvas.gameObject);
				}
			}
			else if (menuSource == MenuSource.UnityUiInScene)
			{
				canvas = Serializer.returnComponent<Canvas>(canvasID, KickStarter.sceneSettings.gameObject);
			}
			if (menuSource != MenuSource.UnityUiInScene || !(canvas == null) || !(runtimeCanvas != null))
			{
				runtimeCanvas = canvas;
			}
		}
	}
}
