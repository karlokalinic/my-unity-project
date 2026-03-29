using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{
	public class OptionalMouseInputModule : StandaloneInputModule
	{
		private bool allowMouseInput = true;

		private readonly MouseState m_MouseState = new MouseState();

		public bool AllowMouseInput
		{
			get
			{
				return allowMouseInput;
			}
			set
			{
				allowMouseInput = value;
			}
		}

		protected void Update()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
			{
				AllowMouseInput = !CanDirectlyControlMenus();
			}
			else
			{
				AllowMouseInput = true;
			}
		}

		protected virtual bool CanDirectlyControlMenus()
		{
			if ((KickStarter.stateHandler.gameState == GameState.Paused && KickStarter.menuManager.keyboardControlWhenPaused) || (KickStarter.stateHandler.gameState == GameState.DialogOptions && KickStarter.menuManager.keyboardControlWhenDialogOptions) || (KickStarter.stateHandler.IsInGameplay() && KickStarter.playerInput.canKeyboardControlMenusDuringGameplay))
			{
				return true;
			}
			return false;
		}

		protected override MouseState GetMousePointerEventData(int id = 0)
		{
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.inputMethod != InputMethod.KeyboardOrController)
			{
				return base.GetMousePointerEventData(id);
			}
			PointerEventData data;
			bool pointerData = GetPointerData(-1, out data, true);
			data.Reset();
			Vector2 mousePosition = KickStarter.playerInput.GetMousePosition();
			if (pointerData)
			{
				data.position = mousePosition;
			}
			data.delta = mousePosition - data.position;
			data.position = mousePosition;
			data.scrollDelta = Input.mouseScrollDelta;
			data.button = PointerEventData.InputButton.Left;
			base.eventSystem.RaycastAll(data, m_RaycastResultCache);
			RaycastResult raycastResult = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
			m_RaycastResultCache.Clear();
			if (raycastResult.isValid && KickStarter.menuManager.autoSelectValidRaycasts && !CanDirectlyControlMenus())
			{
				KickStarter.playerMenus.EventSystem.SetSelectedGameObject(raycastResult.gameObject);
			}
			PointerEventData data2;
			GetPointerData(-2, out data2, true);
			CopyFromTo(data, data2);
			data2.button = PointerEventData.InputButton.Right;
			PointerEventData data3;
			GetPointerData(-3, out data3, true);
			CopyFromTo(data, data3);
			data3.button = PointerEventData.InputButton.Middle;
			PointerEventData.FramePressState stateForMouseButton = PointerEventData.FramePressState.NotChanged;
			if (KickStarter.playerInput.InputGetButtonDown("InteractionA"))
			{
				stateForMouseButton = PointerEventData.FramePressState.Pressed;
			}
			else if (KickStarter.playerInput.InputGetButtonUp("InteractionA"))
			{
				stateForMouseButton = PointerEventData.FramePressState.Released;
			}
			PointerEventData.FramePressState stateForMouseButton2 = PointerEventData.FramePressState.NotChanged;
			if (KickStarter.playerInput.InputGetButtonDown("InteractionB"))
			{
				stateForMouseButton2 = PointerEventData.FramePressState.Pressed;
			}
			else if (KickStarter.playerInput.InputGetButtonUp("InteractionB"))
			{
				stateForMouseButton2 = PointerEventData.FramePressState.Released;
			}
			m_MouseState.SetButtonState(PointerEventData.InputButton.Left, stateForMouseButton, data);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Right, stateForMouseButton2, data2);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data3);
			return m_MouseState;
		}

		public override void Process()
		{
			bool flag = SendUpdateEventToSelectedObject();
			if (base.eventSystem.sendNavigationEvents)
			{
				if (!flag)
				{
					flag |= SendMoveEventToSelectedObject();
				}
				if (!flag)
				{
					SendSubmitEventToSelectedObject();
				}
			}
			if (allowMouseInput)
			{
				ProcessMouseEvent();
			}
		}
	}
}
