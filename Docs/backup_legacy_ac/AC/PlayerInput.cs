using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_input.html")]
	public class PlayerInput : MonoBehaviour
	{
		public delegate bool InputButtonDelegate(string buttonName);

		public delegate float InputAxisDelegate(string axisName);

		public delegate Vector2 InputMouseDelegate(bool cusorIsLocked = false);

		public delegate bool InputMouseButtonDelegate(int button);

		public delegate Vector2 InputTouchDelegate(int index);

		public delegate TouchPhase InputTouchPhaseDelegate(int index);

		public delegate int _InputTouchCountDelegate();

		protected AnimationCurve timeCurve;

		protected float changeTimeStart;

		protected MouseState mouseState;

		protected DragState dragState;

		protected Vector2 moveKeys = new Vector2(0f, 0f);

		protected bool playerIsControlledRunning;

		[HideInInspector]
		public float timeScale = 1f;

		protected bool isUpLocked;

		protected bool isDownLocked;

		protected bool isLeftLocked;

		protected bool isRightLocked;

		protected bool freeAimLock;

		protected bool isJumpLocked;

		[HideInInspector]
		public bool canKeyboardControlMenusDuringGameplay;

		[HideInInspector]
		public PlayerMoveLock runLock;

		[HideInInspector]
		public string skipMovieKey = string.Empty;

		public float clickDelay = 0.3f;

		public float doubleClickDelay = 1f;

		public string dragOverrideInput = string.Empty;

		protected float clickTime;

		protected float doubleClickTime;

		protected MenuDrag activeDragElement;

		protected bool hasUnclickedSinceClick;

		protected bool lastClickWasDouble;

		protected float lastclickTime;

		protected string menuButtonInput;

		protected float menuButtonValue;

		protected SimulateInputType menuInput;

		public float cursorMoveSpeed = 4f;

		[HideInInspector]
		public bool cameraLockSnap;

		protected Vector2 xboxCursor;

		protected Vector2 mousePosition;

		protected bool scrollingLocked;

		protected bool canCycleInteractionInput = true;

		protected Vector2 dragStartPosition = Vector2.zero;

		protected Vector2 dragEndPosition = Vector2.zero;

		protected float dragSpeed;

		protected Vector2 dragVector;

		protected float touchTime;

		protected float touchThreshold = 0.2f;

		protected Vector2 freeAim;

		protected bool toggleCursorOn;

		protected bool cursorIsLocked;

		public ForceGameplayCursor forceGameplayCursor;

		protected bool canDragMoveable;

		protected DragBase dragObject;

		protected Vector2 lastMousePosition;

		protected bool resetMouseDelta;

		protected Vector3 lastCameraPosition;

		protected Vector3 dragForce;

		protected Vector2 deltaDragMouse;

		[HideInInspector]
		public Conversation activeConversation;

		protected Conversation pendingOptionConversation;

		[HideInInspector]
		public ArrowPrompt activeArrows;

		[HideInInspector]
		public Container activeContainer;

		protected bool mouseIsOnScreen = true;

		public InputButtonDelegate InputGetButtonDownDelegate;

		public InputButtonDelegate InputGetButtonUpDelegate;

		public InputButtonDelegate InputGetButtonDelegate;

		public InputAxisDelegate InputGetAxisDelegate;

		public InputMouseButtonDelegate InputGetMouseButtonDelegate;

		public InputMouseButtonDelegate InputGetMouseButtonDownDelegate;

		public InputMouseDelegate InputMousePositionDelegate;

		public InputTouchDelegate InputTouchPositionDelegate;

		public InputTouchDelegate InputTouchDeltaPositionDelegate;

		public InputTouchPhaseDelegate InputGetTouchPhaseDelegate;

		public InputMouseDelegate InputGetFreeAimDelegate;

		public _InputTouchCountDelegate InputTouchCountDelegate;

		protected LerpUtils.Vector2Lerp freeAimLerp = new LerpUtils.Vector2Lerp();

		public bool IsJumpLocked
		{
			get
			{
				return isJumpLocked;
			}
		}

		public Conversation PendingOptionConversation
		{
			get
			{
				return pendingOptionConversation;
			}
			set
			{
				pendingOptionConversation = value;
			}
		}

		protected virtual Vector2 LockedCursorPosition
		{
			get
			{
				return new Vector2((float)ACScreen.width / 2f, (float)ACScreen.height / 2f);
			}
		}

		public void OnAwake()
		{
			if ((bool)KickStarter.settingsManager)
			{
				InitialiseCursorLock(KickStarter.settingsManager.movementMethod);
			}
			ResetClick();
			xboxCursor = LockedCursorPosition;
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.CanDragCursor())
			{
				mousePosition = xboxCursor;
			}
		}

		public void UpdateInput()
		{
			if (timeCurve != null && timeCurve.length > 0)
			{
				float num = Time.time - changeTimeStart;
				if (timeCurve[timeCurve.length - 1].time < num)
				{
					SetTimeScale(timeCurve[timeCurve.length - 1].time);
					timeCurve = null;
				}
				else
				{
					SetTimeScale(timeCurve.Evaluate(num));
				}
			}
			if (clickTime > 0f)
			{
				clickTime -= 4f * GetDeltaTime();
			}
			if (clickTime < 0f)
			{
				clickTime = 0f;
			}
			if (doubleClickTime > 0f)
			{
				doubleClickTime -= 4f * GetDeltaTime();
			}
			if (doubleClickTime < 0f)
			{
				doubleClickTime = 0f;
			}
			bool flag = false;
			if (!string.IsNullOrEmpty(skipMovieKey) && InputGetButtonDown(skipMovieKey) && KickStarter.stateHandler.gameState != GameState.Paused)
			{
				skipMovieKey = string.Empty;
				flag = true;
			}
			if ((bool)KickStarter.stateHandler && (bool)KickStarter.settingsManager)
			{
				lastMousePosition = mousePosition;
				if (InputGetButtonDown("ToggleCursor") && KickStarter.stateHandler.IsInGameplay())
				{
					ToggleCursor();
				}
				if (KickStarter.stateHandler.gameState == GameState.Cutscene && InputGetButtonDown("EndCutscene") && !flag)
				{
					KickStarter.actionListManager.EndCutscene();
				}
				if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
				{
					if (KickStarter.stateHandler.gameState == GameState.Paused || (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowGameplayDuringConversations) || (freeAimLock && KickStarter.settingsManager.IsInFirstPerson()))
					{
						cursorIsLocked = false;
					}
					else if (dragObject != null && KickStarter.settingsManager.IsInFirstPerson() && KickStarter.settingsManager.disableFreeAimWhenDragging)
					{
						cursorIsLocked = false;
					}
					else if (forceGameplayCursor == ForceGameplayCursor.KeepLocked)
					{
						cursorIsLocked = true;
					}
					else if (forceGameplayCursor == ForceGameplayCursor.KeepUnlocked)
					{
						cursorIsLocked = false;
					}
					else
					{
						cursorIsLocked = toggleCursorOn;
					}
					UnityVersionHandler.CursorLock = cursorIsLocked;
					if (cursorIsLocked)
					{
						mousePosition = InputMousePosition(true);
					}
					else
					{
						mousePosition = InputMousePosition(false);
					}
					freeAim = GetSmoothFreeAim(InputGetFreeAim(cursorIsLocked));
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					if (InputGetMouseButtonDown(0) || InputGetButtonDown("InteractionA"))
					{
						if (KickStarter.settingsManager.touchUpWhenPaused && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.stateHandler.gameState == GameState.Paused)
						{
							ResetMouseClick();
						}
						else if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick();
							}
							else if (CanClick())
							{
								dragStartPosition = GetInvertedMouse();
								mouseState = MouseState.SingleClick;
								ResetClick();
								ResetDoubleClick();
							}
						}
					}
					else if (InputGetButtonDown(dragOverrideInput))
					{
						if (KickStarter.stateHandler.IsInGameplay() && mouseState == MouseState.Normal && !CanDoubleClick() && CanClick())
						{
							dragStartPosition = GetInvertedMouse();
						}
					}
					else if (InputGetMouseButtonDown(1) || InputGetButtonDown("InteractionB"))
					{
						mouseState = MouseState.RightClick;
					}
					else if (!string.IsNullOrEmpty(dragOverrideInput) && InputGetButton(dragOverrideInput))
					{
						mouseState = MouseState.HeldDown;
						SetDragState();
					}
					else if (string.IsNullOrEmpty(dragOverrideInput) && (InputGetMouseButton(0) || InputGetButton("InteractionA")))
					{
						mouseState = MouseState.HeldDown;
						SetDragState();
					}
					else if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick())
					{
						if (KickStarter.settingsManager.touchUpWhenPaused && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.stateHandler.gameState == GameState.Paused)
						{
							mouseState = MouseState.SingleClick;
							ResetClick();
							ResetDoubleClick();
						}
						else
						{
							mouseState = MouseState.LetGo;
						}
					}
					else
					{
						ResetMouseClick();
					}
					SetDoubleClickState();
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						if (InputGetFreeAimDelegate != null)
						{
							freeAim = GetSmoothFreeAim(InputGetFreeAim(dragState == DragState.Player));
						}
						else if (dragState == DragState.Player)
						{
							if (KickStarter.settingsManager.IsFirstPersonDragMovement())
							{
								freeAim = GetSmoothFreeAim(new Vector2(dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, 0f));
							}
							else
							{
								freeAim = GetSmoothFreeAim(new Vector2(dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, (0f - dragVector.y) * KickStarter.settingsManager.freeAimTouchSpeed));
							}
						}
						else
						{
							freeAim = GetSmoothFreeAim(Vector2.zero);
						}
					}
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					int num2 = InputTouchCount();
					if (forceGameplayCursor == ForceGameplayCursor.KeepLocked)
					{
						cursorIsLocked = true;
					}
					else if (forceGameplayCursor == ForceGameplayCursor.KeepUnlocked)
					{
						cursorIsLocked = false;
					}
					else
					{
						cursorIsLocked = toggleCursorOn;
					}
					if (cursorIsLocked)
					{
						mousePosition = LockedCursorPosition;
					}
					else if (num2 > 0)
					{
						if (KickStarter.settingsManager.CanDragCursor())
						{
							if (touchTime > touchThreshold && InputTouchPhase(0) == TouchPhase.Moved && num2 == 1)
							{
								mousePosition += InputTouchDeltaPosition(0);
								if (mousePosition.x < 0f)
								{
									mousePosition.x = 0f;
								}
								else if (mousePosition.x > (float)ACScreen.width)
								{
									mousePosition.x = ACScreen.width;
								}
								if (mousePosition.y < 0f)
								{
									mousePosition.y = 0f;
								}
								else if (mousePosition.y > (float)ACScreen.height)
								{
									mousePosition.y = ACScreen.height;
								}
							}
						}
						else
						{
							mousePosition = InputTouchPosition(0);
						}
					}
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					if (touchTime > 0f && touchTime < touchThreshold)
					{
						dragStartPosition = GetInvertedMouse();
					}
					if ((num2 == 1 && KickStarter.stateHandler.gameState == GameState.Cutscene && InputTouchPhase(0) == TouchPhase.Began) || (num2 == 1 && !KickStarter.settingsManager.CanDragCursor() && InputTouchPhase(0) == TouchPhase.Began) || Mathf.Approximately(touchTime, -1f))
					{
						if (KickStarter.settingsManager.touchUpWhenPaused && KickStarter.stateHandler.gameState == GameState.Paused)
						{
							ResetMouseClick();
						}
						else if (mouseState == MouseState.Normal)
						{
							dragStartPosition = GetInvertedMouse();
							if (CanDoubleClick())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick();
							}
							else if (CanClick())
							{
								dragStartPosition = GetInvertedMouse();
								mouseState = MouseState.SingleClick;
								ResetClick();
								ResetDoubleClick();
							}
						}
					}
					else if (num2 == 2 && InputTouchPhase(1) == TouchPhase.Began)
					{
						mouseState = MouseState.RightClick;
						if (KickStarter.settingsManager.IsFirstPersonDragComplex())
						{
							dragStartPosition = GetInvertedMouse();
						}
					}
					else if (num2 == 1 && (InputTouchPhase(0) == TouchPhase.Stationary || InputTouchPhase(0) == TouchPhase.Moved))
					{
						mouseState = MouseState.HeldDown;
						SetDragState();
					}
					else if (num2 == 2 && (InputTouchPhase(0) == TouchPhase.Stationary || InputTouchPhase(0) == TouchPhase.Moved) && KickStarter.settingsManager.IsFirstPersonDragComplex())
					{
						mouseState = MouseState.HeldDown;
						SetDragStateTouchScreen();
					}
					else if (mouseState == MouseState.HeldDown && dragState == DragState.None && CanClick())
					{
						if (KickStarter.settingsManager.touchUpWhenPaused && KickStarter.stateHandler.gameState == GameState.Paused)
						{
							mouseState = MouseState.SingleClick;
							ResetClick();
							ResetDoubleClick();
						}
						else
						{
							mouseState = MouseState.LetGo;
						}
					}
					else
					{
						ResetMouseClick();
					}
					SetDoubleClickState();
					if (KickStarter.settingsManager.CanDragCursor())
					{
						if (num2 > 0)
						{
							touchTime += GetDeltaTime();
						}
						else if (touchTime > 0f && touchTime < touchThreshold)
						{
							touchTime = -1f;
						}
						else
						{
							touchTime = 0f;
						}
					}
					if (InputGetFreeAimDelegate != null)
					{
						freeAim = GetSmoothFreeAim(InputGetFreeAim(dragState == DragState.Player));
					}
					else if (dragState == DragState.Player)
					{
						if (KickStarter.settingsManager.IsFirstPersonDragMovement())
						{
							freeAim = GetSmoothFreeAim(new Vector2(dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, 0f));
						}
						else
						{
							freeAim = GetSmoothFreeAim(new Vector2(dragVector.x * KickStarter.settingsManager.freeAimTouchSpeed, (0f - dragVector.y) * KickStarter.settingsManager.freeAimTouchSpeed));
						}
					}
					else
					{
						freeAim = GetSmoothFreeAim(Vector2.zero);
					}
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					if (freeAimLock && KickStarter.settingsManager.IsInFirstPerson())
					{
						cursorIsLocked = false;
					}
					else if (dragObject != null && KickStarter.settingsManager.IsInFirstPerson() && KickStarter.settingsManager.disableFreeAimWhenDragging)
					{
						cursorIsLocked = false;
					}
					else if (KickStarter.stateHandler.IsInGameplay())
					{
						if (forceGameplayCursor == ForceGameplayCursor.KeepLocked)
						{
							cursorIsLocked = true;
						}
						else if (forceGameplayCursor == ForceGameplayCursor.KeepUnlocked)
						{
							cursorIsLocked = false;
						}
						else
						{
							cursorIsLocked = toggleCursorOn;
						}
					}
					else
					{
						cursorIsLocked = false;
					}
					if (cursorIsLocked)
					{
						mousePosition = LockedCursorPosition;
					}
					else
					{
						if (KickStarter.settingsManager.scaleCursorSpeedWithScreen)
						{
							xboxCursor.x += InputGetAxis("CursorHorizontal") * cursorMoveSpeed * GetDeltaTime() * (float)ACScreen.width * 0.5f;
							xboxCursor.y += InputGetAxis("CursorVertical") * cursorMoveSpeed * GetDeltaTime() * (float)ACScreen.height * 0.5f;
						}
						else
						{
							xboxCursor.x += InputGetAxis("CursorHorizontal") * cursorMoveSpeed * GetDeltaTime() * 300f;
							xboxCursor.y += InputGetAxis("CursorVertical") * cursorMoveSpeed * GetDeltaTime() * 300f;
						}
						xboxCursor.x = Mathf.Clamp(xboxCursor.x, 0f, ACScreen.width);
						xboxCursor.y = Mathf.Clamp(xboxCursor.y, 0f, ACScreen.height);
						mousePosition = xboxCursor;
						freeAim = Vector2.zero;
					}
					freeAim = GetSmoothFreeAim(InputGetFreeAim(cursorIsLocked, 50f));
					if (mouseState == MouseState.Normal)
					{
						dragState = DragState.None;
					}
					if (InputGetButtonDown("InteractionA"))
					{
						if (mouseState == MouseState.Normal)
						{
							if (CanDoubleClick())
							{
								mouseState = MouseState.DoubleClick;
								ResetClick();
							}
							else if (CanClick())
							{
								dragStartPosition = GetInvertedMouse();
								mouseState = MouseState.SingleClick;
								ResetClick();
								ResetDoubleClick();
							}
						}
					}
					else if (InputGetButtonDown(dragOverrideInput))
					{
						if (mouseState == MouseState.Normal && !CanDoubleClick() && CanClick())
						{
							dragStartPosition = GetInvertedMouse();
						}
					}
					else if (InputGetButtonDown("InteractionB"))
					{
						mouseState = MouseState.RightClick;
					}
					else if (!string.IsNullOrEmpty(dragOverrideInput) && InputGetButton(dragOverrideInput))
					{
						mouseState = MouseState.HeldDown;
						SetDragState();
					}
					else if (string.IsNullOrEmpty(dragOverrideInput) && InputGetButton("InteractionA"))
					{
						mouseState = MouseState.HeldDown;
						SetDragState();
					}
					else
					{
						ResetMouseClick();
					}
					SetDoubleClickState();
				}
				if (KickStarter.playerInteraction.GetHotspotMovingTo() != null)
				{
					freeAim = Vector2.zero;
				}
				if (KickStarter.stateHandler.IsInGameplay())
				{
					DetectCursorInputs();
				}
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && InputGetButtonDown("DefaultInteraction") && KickStarter.settingsManager.allowDefaultInventoryInteractions && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.settingsManager.CanSelectItems(false) && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.runtimeInventory.hoverItem != null && KickStarter.playerInteraction.GetActiveHotspot() == null)
				{
					KickStarter.runtimeInventory.hoverItem.RunDefaultInteraction();
					ResetMouseClick();
					ResetClick();
					return;
				}
				if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.playerMenus.IsInteractionMenuOn())
				{
					float num3 = InputGetAxis("CycleInteractions");
					if (InputGetButtonDown("CycleInteractionsRight"))
					{
						KickStarter.playerInteraction.SetNextInteraction();
					}
					else if (InputGetButtonDown("CycleInteractionsLeft"))
					{
						KickStarter.playerInteraction.SetPreviousInteraction();
					}
					if (num3 > 0.1f)
					{
						if (canCycleInteractionInput)
						{
							canCycleInteractionInput = false;
							KickStarter.playerInteraction.SetNextInteraction();
						}
					}
					else if (num3 < -0.1f)
					{
						if (canCycleInteractionInput)
						{
							canCycleInteractionInput = false;
							KickStarter.playerInteraction.SetPreviousInteraction();
						}
					}
					else
					{
						canCycleInteractionInput = true;
					}
				}
				mousePosition = KickStarter.mainCamera.LimitToAspect(mousePosition);
				if (resetMouseDelta)
				{
					lastMousePosition = mousePosition;
					resetMouseDelta = false;
				}
				if (mouseState == MouseState.Normal && !hasUnclickedSinceClick)
				{
					hasUnclickedSinceClick = true;
				}
				if (mouseState == MouseState.Normal)
				{
					canDragMoveable = true;
				}
				UpdateDrag();
				if (dragState != DragState.None)
				{
					dragVector = GetInvertedMouse() - dragStartPosition;
					dragSpeed = dragVector.magnitude;
				}
				else
				{
					dragVector = Vector2.zero;
					dragSpeed = 0f;
				}
				UpdateActiveInputs();
				if (mousePosition.x < 0f || mousePosition.x > (float)ACScreen.width || mousePosition.y < 0f || mousePosition.y > (float)ACScreen.height)
				{
					mouseIsOnScreen = false;
				}
				else
				{
					mouseIsOnScreen = true;
				}
			}
			UpdateDragLine();
		}

		protected void SetDoubleClickState()
		{
			if (mouseState == MouseState.DoubleClick)
			{
				lastClickWasDouble = true;
			}
			else if (mouseState == MouseState.SingleClick || mouseState == MouseState.RightClick || mouseState == MouseState.LetGo)
			{
				lastClickWasDouble = false;
			}
			if (mouseState == MouseState.DoubleClick || mouseState == MouseState.RightClick || mouseState == MouseState.SingleClick)
			{
				lastclickTime = clickDelay;
			}
			else if (lastclickTime > 0f)
			{
				lastclickTime -= Time.deltaTime;
			}
		}

		public bool ClickedRecently(bool checkForDouble = false)
		{
			if (lastclickTime > 0f && checkForDouble == lastClickWasDouble)
			{
				return true;
			}
			return false;
		}

		protected void UpdateActiveInputs()
		{
			if (KickStarter.settingsManager.activeInputs != null)
			{
				for (int i = 0; i < KickStarter.settingsManager.activeInputs.Count && !KickStarter.settingsManager.activeInputs[i].TestForInput(); i++)
				{
				}
			}
		}

		protected void DetectCursorInputs()
		{
			if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot || !KickStarter.cursorManager.allowIconInput)
			{
				return;
			}
			if (KickStarter.cursorManager.allowWalkCursor && InputGetButtonDown("Icon_Walk"))
			{
				KickStarter.runtimeInventory.SetNull();
				KickStarter.playerCursor.ResetSelectedCursor();
				return;
			}
			foreach (CursorIcon cursorIcon in KickStarter.cursorManager.cursorIcons)
			{
				if (InputGetButtonDown(cursorIcon.GetButtonName()))
				{
					KickStarter.runtimeInventory.SetNull();
					KickStarter.playerCursor.SetCursor(cursorIcon);
					break;
				}
			}
		}

		public Vector2 GetMousePosition()
		{
			return mousePosition;
		}

		public Vector2 GetInvertedMouse()
		{
			return new Vector2(GetMousePosition().x, (float)ACScreen.height - GetMousePosition().y);
		}

		public void SetSimulatedCursorPosition(Vector2 newPosition)
		{
			xboxCursor = newPosition;
			if (!cursorIsLocked)
			{
				mousePosition = xboxCursor;
			}
		}

		public void InitialiseCursorLock(MovementMethod movementMethod)
		{
			if (KickStarter.settingsManager.IsInFirstPerson() && movementMethod != MovementMethod.FirstPerson)
			{
				toggleCursorOn = false;
				return;
			}
			toggleCursorOn = KickStarter.settingsManager.lockCursorOnStart;
			if (toggleCursorOn && !KickStarter.settingsManager.IsInFirstPerson() && KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && KickStarter.settingsManager.hotspotDetection == HotspotDetection.MouseOver)
			{
				ACDebug.Log("Starting a non-First Person game with a locked cursor - is this correct?");
			}
		}

		public bool IsCursorReadable()
		{
			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && mouseState == MouseState.Normal)
			{
				if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.InventoryDragDrop)
				{
					return true;
				}
				return KickStarter.settingsManager.CanDragCursor();
			}
			return true;
		}

		public void DetectConversationNumerics()
		{
			if (!(activeConversation != null) || !KickStarter.settingsManager.runConversationsWithKeys)
			{
				return;
			}
			Event current = Event.current;
			if (current.isKey && current.type == EventType.KeyDown)
			{
				if (current.keyCode == KeyCode.Alpha1 || current.keyCode == KeyCode.Keypad1)
				{
					activeConversation.RunOption(0);
				}
				else if (current.keyCode == KeyCode.Alpha2 || current.keyCode == KeyCode.Keypad2)
				{
					activeConversation.RunOption(1);
				}
				else if (current.keyCode == KeyCode.Alpha3 || current.keyCode == KeyCode.Keypad3)
				{
					activeConversation.RunOption(2);
				}
				else if (current.keyCode == KeyCode.Alpha4 || current.keyCode == KeyCode.Keypad4)
				{
					activeConversation.RunOption(3);
				}
				else if (current.keyCode == KeyCode.Alpha5 || current.keyCode == KeyCode.Keypad5)
				{
					activeConversation.RunOption(4);
				}
				else if (current.keyCode == KeyCode.Alpha6 || current.keyCode == KeyCode.Keypad6)
				{
					activeConversation.RunOption(5);
				}
				else if (current.keyCode == KeyCode.Alpha7 || current.keyCode == KeyCode.Keypad7)
				{
					activeConversation.RunOption(6);
				}
				else if (current.keyCode == KeyCode.Alpha8 || current.keyCode == KeyCode.Keypad8)
				{
					activeConversation.RunOption(7);
				}
				else if (current.keyCode == KeyCode.Alpha9 || current.keyCode == KeyCode.Keypad9)
				{
					activeConversation.RunOption(8);
				}
			}
		}

		public void DetectConversationInputs()
		{
			if (activeConversation != null && KickStarter.settingsManager.runConversationsWithKeys)
			{
				if (InputGetButtonDown("DialogueOption1"))
				{
					activeConversation.RunOption(0);
				}
				else if (InputGetButtonDown("DialogueOption2"))
				{
					activeConversation.RunOption(1);
				}
				else if (InputGetButtonDown("DialogueOption3"))
				{
					activeConversation.RunOption(2);
				}
				else if (InputGetButtonDown("DialogueOption4"))
				{
					activeConversation.RunOption(3);
				}
				else if (InputGetButtonDown("DialogueOption5"))
				{
					activeConversation.RunOption(4);
				}
				else if (InputGetButtonDown("DialogueOption6"))
				{
					activeConversation.RunOption(5);
				}
				else if (InputGetButtonDown("DialogueOption7"))
				{
					activeConversation.RunOption(6);
				}
				else if (InputGetButtonDown("DialogueOption8"))
				{
					activeConversation.RunOption(7);
				}
				else if (InputGetButtonDown("DialogueOption9"))
				{
					activeConversation.RunOption(8);
				}
			}
		}

		public void DrawDragLine()
		{
			if (KickStarter.settingsManager.drawDragLine && dragEndPosition != Vector2.zero)
			{
				DrawStraightLine.Draw(dragStartPosition, dragEndPosition, KickStarter.settingsManager.dragLineColor, KickStarter.settingsManager.dragLineWidth, true);
			}
		}

		protected void UpdateDragLine()
		{
			dragEndPosition = Vector2.zero;
			if (dragState == DragState.Player && KickStarter.settingsManager.movementMethod != MovementMethod.StraightToCursor)
			{
				dragEndPosition = GetInvertedMouse();
				KickStarter.eventManager.Call_OnUpdateDragLine(dragStartPosition, dragEndPosition);
			}
			else
			{
				KickStarter.eventManager.Call_OnUpdateDragLine(Vector2.zero, Vector2.zero);
			}
			if (!(activeDragElement != null))
			{
				return;
			}
			if (mouseState == MouseState.HeldDown)
			{
				if (!activeDragElement.DoDrag(GetDragVector()))
				{
					activeDragElement = null;
				}
			}
			else if (mouseState == MouseState.Normal && activeDragElement.CheckStop(GetInvertedMouse()))
			{
				activeDragElement = null;
			}
		}

		public void UpdateDirectInput()
		{
			if (!(KickStarter.settingsManager != null))
			{
				return;
			}
			if (activeArrows != null)
			{
				if (activeArrows.arrowPromptType == ArrowPromptType.KeyOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick)
				{
					Vector2 vector = new Vector2(InputGetAxis("Horizontal"), 0f - InputGetAxis("Vertical"));
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && dragState == DragState.ScreenArrows)
					{
						vector = GetDragVector() / KickStarter.settingsManager.dragRunThreshold / KickStarter.settingsManager.dragWalkThreshold;
					}
					if (vector.sqrMagnitude > 0f)
					{
						float num = 0.95f;
						if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
						{
							num = 0.05f;
						}
						if (vector.x > num)
						{
							activeArrows.DoRight();
						}
						else if (vector.x < 0f - num)
						{
							activeArrows.DoLeft();
						}
						else if (vector.y < 0f - num)
						{
							activeArrows.DoUp();
						}
						else if (vector.y > num)
						{
							activeArrows.DoDown();
						}
					}
				}
				if (activeArrows != null && (activeArrows.arrowPromptType == ArrowPromptType.ClickOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick))
				{
					Vector2 invertedMouse = GetInvertedMouse();
					if (mouseState == MouseState.SingleClick)
					{
						if (activeArrows.upArrow.rect.Contains(invertedMouse))
						{
							activeArrows.DoUp();
						}
						else if (activeArrows.downArrow.rect.Contains(invertedMouse))
						{
							activeArrows.DoDown();
						}
						else if (activeArrows.leftArrow.rect.Contains(invertedMouse))
						{
							activeArrows.DoLeft();
						}
						else if (activeArrows.rightArrow.rect.Contains(invertedMouse))
						{
							activeArrows.DoRight();
						}
					}
				}
			}
			if (activeArrows == null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
			{
				float num2 = 0f;
				float num3 = 0f;
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
				{
					if (KickStarter.settingsManager.IsInFirstPerson() && KickStarter.settingsManager.firstPersonTouchScreen == FirstPersonTouchScreen.CustomInput)
					{
						num2 = InputGetAxis("Horizontal");
						num3 = InputGetAxis("Vertical");
					}
					else if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.settingsManager.directTouchScreen == DirectTouchScreen.CustomInput)
					{
						num2 = InputGetAxis("Horizontal");
						num3 = InputGetAxis("Vertical");
					}
					else if (dragState != DragState.None)
					{
						num2 = dragVector.x;
						num3 = 0f - dragVector.y;
					}
				}
				else
				{
					num2 = InputGetAxis("Horizontal");
					num3 = InputGetAxis("Vertical");
				}
				if ((isUpLocked && num3 > 0f) || (isDownLocked && num3 < 0f))
				{
					num3 = 0f;
				}
				if ((isLeftLocked && num2 > 0f) || (isRightLocked && num2 < 0f))
				{
					num2 = 0f;
				}
				bool flag;
				if (runLock != PlayerMoveLock.Free)
				{
					flag = ((runLock != PlayerMoveLock.AlwaysWalk) ? true : false);
				}
				else if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
				{
					flag = ((dragStartPosition != Vector2.zero && dragSpeed > KickStarter.settingsManager.dragRunThreshold * 10f) ? true : false);
				}
				else
				{
					flag = InputGetAxis("Run") > 0.1f || InputGetButton("Run");
					if (InputGetButtonDown("ToggleRun") && (bool)KickStarter.player)
					{
						KickStarter.player.toggleRun = !KickStarter.player.toggleRun;
					}
				}
				if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen && (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson || KickStarter.settingsManager.movementMethod == MovementMethod.Direct) && runLock == PlayerMoveLock.Free && (bool)KickStarter.player && KickStarter.player.toggleRun)
				{
					playerIsControlledRunning = !flag;
				}
				else
				{
					playerIsControlledRunning = flag;
				}
				moveKeys = CreateMoveKeys(num2, num3);
			}
			if (InputGetButtonDown("FlashHotspots"))
			{
				FlashHotspots();
			}
		}

		protected Vector2 CreateMoveKeys(float h, float v)
		{
			if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen && KickStarter.settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
			{
				if (KickStarter.settingsManager.limitDirectMovement == LimitDirectMovement.FourDirections)
				{
					if (Mathf.Abs(h) > Mathf.Abs(v))
					{
						v = 0f;
					}
					else
					{
						h = 0f;
					}
				}
				else if (KickStarter.settingsManager.limitDirectMovement == LimitDirectMovement.EightDirections)
				{
					if (!(Mathf.Abs(h) > Mathf.Abs(v)))
					{
						h = ((Mathf.Abs(h) < Mathf.Abs(v)) ? 0f : ((!(Mathf.Abs(h) > 0.4f) || !(Mathf.Abs(v) > 0.4f)) ? (v = 0f) : ((!(h * v > 0f)) ? (0f - v) : v)));
					}
					else
					{
						v = 0f;
					}
				}
			}
			if (cameraLockSnap)
			{
				Vector2 vector = new Vector2(h, v);
				if (vector.sqrMagnitude < 0.01f || Vector2.Angle(vector, moveKeys) > 5f)
				{
					cameraLockSnap = false;
					return vector;
				}
				return moveKeys;
			}
			return new Vector2(h, v);
		}

		protected virtual void FlashHotspots()
		{
			Hotspot[] array = KickStarter.stateHandler.Hotspots.ToArray();
			Hotspot[] array2 = array;
			foreach (Hotspot hotspot in array2)
			{
				if ((bool)hotspot.highlight && hotspot.IsOn() && hotspot.PlayerIsWithinBoundary() && hotspot != KickStarter.playerInteraction.GetActiveHotspot())
				{
					hotspot.highlight.Flash();
				}
			}
		}

		public void RemoveActiveArrows()
		{
			if ((bool)activeArrows)
			{
				activeArrows.TurnOff();
			}
		}

		public void ResetClick()
		{
			clickTime = clickDelay;
			hasUnclickedSinceClick = false;
		}

		protected void ResetDoubleClick()
		{
			doubleClickTime = doubleClickDelay;
		}

		public bool CanClick()
		{
			if (clickTime <= 0f)
			{
				return true;
			}
			return false;
		}

		public bool CanDoubleClick()
		{
			if (doubleClickTime > 0f && clickTime <= 0f)
			{
				return true;
			}
			return false;
		}

		public void SimulateInputButton(string button)
		{
			SimulateInput(SimulateInputType.Button, button, 1f);
		}

		public void SimulateInputAxis(string axis, float value)
		{
			SimulateInput(SimulateInputType.Axis, axis, value);
		}

		public void SimulateInput(SimulateInputType input, string axis, float value)
		{
			if (!string.IsNullOrEmpty(axis))
			{
				menuInput = input;
				menuButtonInput = axis;
				if (input == SimulateInputType.Button)
				{
					menuButtonValue = 1f;
				}
				else
				{
					menuButtonValue = value;
				}
				CancelInvoke();
				Invoke("StopSimulatingInput", 0.1f);
			}
		}

		public bool IsCursorLocked()
		{
			return cursorIsLocked;
		}

		protected void StopSimulatingInput()
		{
			menuButtonInput = string.Empty;
		}

		public bool InputAnyKey()
		{
			if (menuButtonInput != null && !string.IsNullOrEmpty(menuButtonInput))
			{
				return true;
			}
			return Input.anyKey;
		}

		protected float InputGetAxisRaw(string axis)
		{
			if (string.IsNullOrEmpty(axis))
			{
				return 0f;
			}
			if (InputGetAxisDelegate != null)
			{
				return InputGetAxisDelegate(axis);
			}
			if (KickStarter.settingsManager.assumeInputsDefined)
			{
				if (!Mathf.Approximately(Input.GetAxisRaw(axis), 0f))
				{
					return Input.GetAxisRaw(axis);
				}
			}
			else
			{
				try
				{
					if (!Mathf.Approximately(Input.GetAxisRaw(axis), 0f))
					{
						return Input.GetAxisRaw(axis);
					}
				}
				catch
				{
				}
			}
			if (!string.IsNullOrEmpty(menuButtonInput) && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			return 0f;
		}

		public float InputGetAxis(string axis)
		{
			if (string.IsNullOrEmpty(axis))
			{
				return 0f;
			}
			if (InputGetAxisDelegate != null)
			{
				return InputGetAxisDelegate(axis);
			}
			if (KickStarter.settingsManager.assumeInputsDefined)
			{
				if (!Mathf.Approximately(Input.GetAxis(axis), 0f))
				{
					return Input.GetAxis(axis);
				}
			}
			else
			{
				try
				{
					if (!Mathf.Approximately(Input.GetAxis(axis), 0f))
					{
						return Input.GetAxis(axis);
					}
				}
				catch
				{
				}
			}
			if (!string.IsNullOrEmpty(menuButtonInput) && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			return 0f;
		}

		protected bool InputGetMouseButton(int button)
		{
			if (InputGetMouseButtonDelegate != null)
			{
				return InputGetMouseButtonDelegate(button);
			}
			if (KickStarter.settingsManager.inputMethod != InputMethod.MouseAndKeyboard || KickStarter.settingsManager.defaultMouseClicks)
			{
				return Input.GetMouseButton(button);
			}
			return false;
		}

		protected Vector2 InputMousePosition(bool _cursorIsLocked)
		{
			if (InputMousePositionDelegate != null)
			{
				return InputMousePositionDelegate(_cursorIsLocked);
			}
			if (_cursorIsLocked)
			{
				return LockedCursorPosition;
			}
			return Input.mousePosition;
		}

		protected Vector2 InputTouchPosition(int index)
		{
			if (InputTouchPositionDelegate != null)
			{
				return InputTouchPositionDelegate(index);
			}
			if (InputTouchCount() > index)
			{
				return Input.GetTouch(index).position;
			}
			return Vector2.zero;
		}

		protected Vector2 InputTouchDeltaPosition(int index)
		{
			if (InputTouchPositionDelegate != null)
			{
				return InputTouchDeltaPositionDelegate(index);
			}
			if (InputTouchCount() > index)
			{
				Touch touch = Input.GetTouch(0);
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					return touch.deltaPosition * 1.7f;
				}
				return touch.deltaPosition * Time.deltaTime / touch.deltaTime;
			}
			return Vector2.zero;
		}

		protected TouchPhase InputTouchPhase(int index)
		{
			if (InputGetTouchPhaseDelegate != null)
			{
				return InputGetTouchPhaseDelegate(index);
			}
			return Input.GetTouch(index).phase;
		}

		protected int InputTouchCount()
		{
			if (InputTouchCountDelegate != null)
			{
				return InputTouchCountDelegate();
			}
			return Input.touchCount;
		}

		protected Vector2 InputGetFreeAim(bool _cursorIsLocked, float scaleFactor = 1f)
		{
			if (InputGetFreeAimDelegate != null)
			{
				return InputGetFreeAimDelegate(_cursorIsLocked);
			}
			if (_cursorIsLocked)
			{
				return new Vector2(InputGetAxis("CursorHorizontal") * scaleFactor, InputGetAxis("CursorVertical") * scaleFactor);
			}
			return Vector2.zero;
		}

		protected bool InputGetMouseButtonDown(int button)
		{
			if (InputGetMouseButtonDownDelegate != null)
			{
				return InputGetMouseButtonDownDelegate(button);
			}
			if (KickStarter.settingsManager.inputMethod != InputMethod.MouseAndKeyboard || KickStarter.settingsManager.defaultMouseClicks)
			{
				return Input.GetMouseButtonDown(button);
			}
			return false;
		}

		public bool InputGetButton(string axis)
		{
			if (string.IsNullOrEmpty(axis))
			{
				return false;
			}
			if (InputGetButtonDelegate != null)
			{
				return InputGetButtonDelegate(axis);
			}
			if (KickStarter.settingsManager.assumeInputsDefined)
			{
				if (Input.GetButton(axis))
				{
					return true;
				}
			}
			else
			{
				try
				{
					if (Input.GetButton(axis))
					{
						return true;
					}
				}
				catch
				{
				}
			}
			if (!string.IsNullOrEmpty(menuButtonInput) && menuButtonInput == axis && menuInput == SimulateInputType.Button)
			{
				if (menuButtonValue > 0f)
				{
					StopSimulatingInput();
					return true;
				}
				StopSimulatingInput();
			}
			return false;
		}

		public bool InputGetButtonDown(string axis, bool showError = false)
		{
			if (string.IsNullOrEmpty(axis))
			{
				return false;
			}
			if (InputGetButtonDownDelegate != null)
			{
				return InputGetButtonDownDelegate(axis);
			}
			if (KickStarter.settingsManager.assumeInputsDefined)
			{
				if (Input.GetButtonDown(axis))
				{
					return true;
				}
			}
			else
			{
				try
				{
					if (Input.GetButtonDown(axis))
					{
						return true;
					}
				}
				catch
				{
					if (showError)
					{
						ACDebug.LogWarning("Cannot find Input button '" + axis + "' - please define it in Unity's Input Manager (Edit -> Project settings -> Input).");
					}
				}
			}
			if (!string.IsNullOrEmpty(menuButtonInput) && menuButtonInput == axis && menuInput == SimulateInputType.Button)
			{
				if (menuButtonValue > 0f)
				{
					StopSimulatingInput();
					return true;
				}
				StopSimulatingInput();
			}
			return false;
		}

		public bool InputGetButtonUp(string axis)
		{
			if (string.IsNullOrEmpty(axis))
			{
				return false;
			}
			if (InputGetButtonUpDelegate != null)
			{
				return InputGetButtonUpDelegate(axis);
			}
			if (KickStarter.settingsManager.assumeInputsDefined)
			{
				if (Input.GetButtonUp(axis))
				{
					return true;
				}
			}
			else
			{
				try
				{
					if (Input.GetButtonUp(axis))
					{
						return true;
					}
				}
				catch
				{
				}
			}
			return false;
		}

		protected void SetDragState()
		{
			DragState dragState = this.dragState;
			if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.InventoryDragDrop && (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.Paused))
			{
				if (dragVector.magnitude >= KickStarter.settingsManager.dragDropThreshold)
				{
					this.dragState = DragState.Inventory;
				}
				else
				{
					this.dragState = DragState.PreInventory;
				}
			}
			else if (activeDragElement != null && (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.Paused))
			{
				this.dragState = DragState.Menu;
			}
			else if (activeArrows != null && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				this.dragState = DragState.ScreenArrows;
			}
			else if (dragObject != null)
			{
				this.dragState = DragState.Moveable;
			}
			else if ((bool)KickStarter.mainCamera.attachedCamera && KickStarter.mainCamera.attachedCamera.isDragControlled)
			{
				if (!KickStarter.playerInteraction.IsMouseOverHotspot())
				{
					this.dragState = DragState._Camera;
					if (!cursorIsLocked && deltaDragMouse.magnitude * Time.deltaTime <= 1f && (GetInvertedMouse() - dragStartPosition).magnitude < 10f)
					{
						this.dragState = DragState.None;
					}
				}
			}
			else if ((KickStarter.settingsManager.movementMethod == MovementMethod.Drag || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor || (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)) && KickStarter.settingsManager.movementMethod != MovementMethod.None && KickStarter.stateHandler.IsInGameplay())
			{
				if (!KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerMenus.IsInteractionMenuOn() && !KickStarter.playerInteraction.IsMouseOverHotspot())
				{
					this.dragState = DragState.Player;
				}
			}
			else
			{
				this.dragState = DragState.None;
			}
			if (dragState == DragState.None && this.dragState != DragState.None)
			{
				resetMouseDelta = true;
				lastMousePosition = mousePosition;
			}
		}

		protected void SetDragStateTouchScreen()
		{
			if ((KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.InventoryDragDrop && (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.Paused)) || (activeDragElement != null && (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.Paused)) || (activeArrows != null && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen) || dragObject != null || ((bool)KickStarter.mainCamera.attachedCamera && KickStarter.mainCamera.attachedCamera.isDragControlled))
			{
				return;
			}
			if ((KickStarter.settingsManager.movementMethod == MovementMethod.Drag || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor || (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)) && KickStarter.settingsManager.movementMethod != MovementMethod.None && KickStarter.stateHandler.IsInGameplay())
			{
				if (!KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerMenus.IsInteractionMenuOn() && !KickStarter.playerInteraction.IsMouseOverHotspot())
				{
					dragState = DragState.Player;
				}
			}
			else
			{
				dragState = DragState.None;
			}
		}

		protected void UpdateDrag()
		{
			if (dragState != DragState.None)
			{
				if (freeAim.sqrMagnitude > 0f)
				{
					deltaDragMouse = freeAim * 500f / Time.deltaTime;
				}
				else
				{
					deltaDragMouse = (mousePosition - lastMousePosition) / Time.deltaTime;
				}
			}
			if ((bool)dragObject && KickStarter.stateHandler.gameState != GameState.Normal)
			{
				LetGo();
			}
			else if (mouseState == MouseState.HeldDown && dragState == DragState.None && KickStarter.stateHandler.CanInteractWithDraggables() && !KickStarter.playerMenus.IsMouseOverMenu())
			{
				Grab();
			}
			else if (dragState == DragState.Moveable)
			{
				if ((bool)dragObject && (!dragObject.IsHeld || !dragObject.IsOnScreen() || !dragObject.IsCloseToCamera(KickStarter.settingsManager.moveableRaycastLength)))
				{
					LetGo();
				}
			}
			else if ((bool)dragObject)
			{
				LetGo();
			}
		}

		public void _FixedUpdate()
		{
			if ((mouseState != MouseState.HeldDown || dragState != DragState.None || !KickStarter.stateHandler.CanInteract() || KickStarter.playerMenus.IsMouseOverMenu()) && dragState == DragState.Moveable && (bool)dragObject && dragObject.IsHeld && dragObject.IsOnScreen() && dragObject.IsCloseToCamera(KickStarter.settingsManager.moveableRaycastLength))
			{
				Drag();
			}
		}

		public void SetFreeAimLock(bool _state)
		{
			freeAimLock = _state;
		}

		public void LetGo()
		{
			if (dragObject != null)
			{
				dragObject.LetGo();
				dragObject = null;
			}
		}

		protected void Grab()
		{
			if ((bool)dragObject)
			{
				dragObject.LetGo();
				dragObject = null;
			}
			else
			{
				if (!canDragMoveable)
				{
					return;
				}
				canDragMoveable = false;
				Ray ray = KickStarter.CameraMain.ScreenPointToRay(mousePosition);
				RaycastHit hitInfo = default(RaycastHit);
				if (Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.moveableRaycastLength))
				{
					DragBase component = hitInfo.transform.GetComponent<DragBase>();
					if (component != null && component.PlayerIsWithinBoundary())
					{
						dragObject = component;
						dragObject.Grab(hitInfo.point);
						lastCameraPosition = KickStarter.CameraMain.transform.position;
						KickStarter.eventManager.Call_OnGrabMoveable(dragObject);
					}
				}
			}
		}

		protected void Drag()
		{
			if (dragObject.invertInput)
			{
				dragForce = -KickStarter.CameraMain.transform.right * deltaDragMouse.x + -KickStarter.CameraMain.transform.up * deltaDragMouse.y;
			}
			else
			{
				dragForce = KickStarter.CameraMain.transform.right * deltaDragMouse.x + KickStarter.CameraMain.transform.up * deltaDragMouse.y;
			}
			float magnitude = (KickStarter.CameraMain.transform.position - dragObject.transform.position).magnitude;
			if (dragObject.playerMovementInfluence > 0f)
			{
				Vector3 vector = KickStarter.CameraMain.transform.position - lastCameraPosition;
				dragForce += vector * 100000f * dragObject.playerMovementInfluence;
			}
			dragForce /= Time.fixedDeltaTime * 50f;
			dragObject.ApplyDragForce(dragForce, mousePosition, magnitude);
			lastCameraPosition = KickStarter.CameraMain.transform.position;
		}

		public Vector2 GetDragVector()
		{
			if (dragState == DragState._Camera)
			{
				return deltaDragMouse;
			}
			return dragVector;
		}

		public void SetUpLock(bool state)
		{
			isUpLocked = state;
		}

		public void SetLeftLock(bool state)
		{
			isLeftLocked = state;
		}

		public void SetRightLock(bool state)
		{
			isRightLocked = state;
		}

		public void SetDownLock(bool state)
		{
			isDownLocked = state;
		}

		public void SetJumpLock(bool state)
		{
			isJumpLocked = state;
		}

		public bool CanDirectControlPlayer()
		{
			return !isUpLocked;
		}

		public bool ActiveArrowsDisablingHotspots()
		{
			if (activeArrows != null && activeArrows.disableHotspots)
			{
				return true;
			}
			return false;
		}

		protected void ToggleCursor()
		{
			if (!(dragObject != null) || dragObject.CanToggleCursor())
			{
				toggleCursorOn = !toggleCursorOn;
			}
		}

		public void SetInGameCursorState(bool lockState)
		{
			toggleCursorOn = lockState;
		}

		public bool GetInGameCursorState()
		{
			return toggleCursorOn;
		}

		public bool IsDragObjectHeld(DragBase _dragBase)
		{
			if (_dragBase == null || dragObject == null)
			{
				return false;
			}
			if (_dragBase == dragObject)
			{
				return true;
			}
			return false;
		}

		public bool IsDragObjectHeld()
		{
			return dragObject != null;
		}

		public float GetDragMovementSlowDown()
		{
			if (dragObject != null)
			{
				return 1f - dragObject.playerMovementReductionFactor;
			}
			return 1f;
		}

		protected float GetDeltaTime()
		{
			return Time.unscaledDeltaTime;
		}

		public void SetTimeScale(float _timeScale)
		{
			if (_timeScale > 0f)
			{
				timeScale = _timeScale;
				if (KickStarter.stateHandler.gameState != GameState.Paused)
				{
					Time.timeScale = _timeScale;
				}
			}
		}

		public void SetTimeCurve(AnimationCurve _timeCurve)
		{
			timeCurve = _timeCurve;
			changeTimeStart = Time.time;
		}

		public bool HasTimeCurve()
		{
			if (timeCurve != null)
			{
				return true;
			}
			return false;
		}

		public DragState GetDragState()
		{
			return dragState;
		}

		public MouseState GetMouseState()
		{
			return mouseState;
		}

		public void ResetMouseClick()
		{
			mouseState = MouseState.Normal;
		}

		public Vector2 GetMoveKeys()
		{
			return moveKeys;
		}

		public bool IsPlayerControlledRunning()
		{
			return playerIsControlledRunning;
		}

		public void SetActiveDragElement(MenuDrag menuDrag)
		{
			activeDragElement = menuDrag;
		}

		public bool LastClickWasDouble()
		{
			return lastClickWasDouble;
		}

		public void ResetDragMovement()
		{
			dragSpeed = 0f;
		}

		public bool IsDragMoveSpeedOverWalkThreshold()
		{
			if (dragSpeed > KickStarter.settingsManager.dragWalkThreshold * 10f)
			{
				return true;
			}
			return false;
		}

		public bool IsMouseOnScreen()
		{
			return mouseIsOnScreen;
		}

		public Vector2 GetFreeAim()
		{
			return freeAim;
		}

		protected virtual Vector2 GetSmoothFreeAim(Vector2 targetFreeAim)
		{
			if (KickStarter.settingsManager.freeAimSmoothSpeed <= 0f)
			{
				return targetFreeAim;
			}
			float num = 1f;
			if (dragObject != null)
			{
				num = 1f - dragObject.playerMovementReductionFactor;
			}
			return freeAimLerp.Update(freeAim, targetFreeAim * num, KickStarter.settingsManager.freeAimSmoothSpeed);
		}

		public bool IsFreeAimingLocked()
		{
			return freeAimLock;
		}

		public bool AllDirectionsLocked()
		{
			if (isDownLocked && isUpLocked && isLeftLocked && isRightLocked)
			{
				return true;
			}
			return false;
		}

		public void ReturnToGameplayAfterLoad()
		{
			pendingOptionConversation = null;
			if ((bool)activeConversation)
			{
				KickStarter.stateHandler.gameState = GameState.DialogOptions;
			}
			else
			{
				KickStarter.stateHandler.gameState = GameState.Normal;
			}
			ResetMouseClick();
		}

		public MainData SaveMainData(MainData mainData)
		{
			mainData.timeScale = KickStarter.playerInput.timeScale;
			mainData.activeArrows = ((activeArrows != null) ? Serializer.GetConstantID(activeArrows.gameObject) : 0);
			mainData.activeConversation = ((activeConversation != null) ? Serializer.GetConstantID(activeConversation.gameObject) : 0);
			mainData.canKeyboardControlMenusDuringGameplay = canKeyboardControlMenusDuringGameplay;
			return mainData;
		}

		public void LoadMainData(MainData mainData)
		{
			RemoveActiveArrows();
			ArrowPrompt arrowPrompt = Serializer.returnComponent<ArrowPrompt>(mainData.activeArrows);
			if ((bool)arrowPrompt)
			{
				arrowPrompt.TurnOn();
			}
			activeConversation = Serializer.returnComponent<Conversation>(mainData.activeConversation);
			pendingOptionConversation = null;
			timeScale = mainData.timeScale;
			canKeyboardControlMenusDuringGameplay = mainData.canKeyboardControlMenusDuringGameplay;
			if (mainData.toggleCursorState > 0)
			{
				toggleCursorOn = mainData.toggleCursorState == 1;
			}
		}

		public PlayerData SavePlayerData(PlayerData playerData)
		{
			playerData.playerUpLock = isUpLocked;
			playerData.playerDownLock = isDownLocked;
			playerData.playerLeftlock = isLeftLocked;
			playerData.playerRightLock = isRightLocked;
			playerData.playerRunLock = (int)runLock;
			playerData.playerFreeAimLock = IsFreeAimingLocked();
			return playerData;
		}

		public void LoadPlayerData(PlayerData playerData)
		{
			SetUpLock(playerData.playerUpLock);
			isDownLocked = playerData.playerDownLock;
			isLeftLocked = playerData.playerLeftlock;
			isRightLocked = playerData.playerRightLock;
			runLock = (PlayerMoveLock)playerData.playerRunLock;
			SetFreeAimLock(playerData.playerFreeAimLock);
		}

		public virtual void InputControlMenu(Menu menu)
		{
			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || menu.menuSource != MenuSource.AdventureCreator || !menu.IsOn() || !menu.CanCurrentlyKeyboardControl())
			{
				return;
			}
			menu.AutoSelect();
			if (!KickStarter.playerMenus.IsCyclingInteractionMenu() && (KickStarter.stateHandler.gameState == GameState.DialogOptions || KickStarter.stateHandler.gameState == GameState.Paused || (KickStarter.stateHandler.IsInGameplay() && canKeyboardControlMenusDuringGameplay)))
			{
				Vector2 inputDirection = new Vector2(InputGetAxisRaw("Horizontal"), InputGetAxisRaw("Vertical"));
				scrollingLocked = menu.GetNextSlot(inputDirection, scrollingLocked);
				if (InputGetAxisRaw("Vertical") < 0.05f && InputGetAxisRaw("Vertical") > -0.05f && InputGetAxisRaw("Horizontal") < 0.05f && InputGetAxisRaw("Horizontal") > -0.05f)
				{
					scrollingLocked = false;
				}
			}
		}

		public void EndConversation()
		{
			activeConversation = null;
		}

		public bool IsInConversation(bool alsoPendingOption = false)
		{
			if (activeConversation != null)
			{
				return true;
			}
			if (pendingOptionConversation != null && alsoPendingOption)
			{
				return true;
			}
			return false;
		}
	}
}
