using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/3rd-party/Click by hovering cursor example")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_click_by_hovering_cursor_example.html")]
	public class ClickByHoveringCursorExample : MonoBehaviour
	{
		public float hoverDuration = 1f;

		private float hotspotHoverDuration;

		private float menuHoverDuration;

		private string elementOverIdentifier;

		private Hotspot hoverHotspot;

		private Menu hoverMenu;

		private MenuElement hoverElement;

		private int hoverSlot;

		private void OnEnable()
		{
			EventManager.OnHotspotSelect = (EventManager.Delegate_ChangeHotspot)Delegate.Combine(EventManager.OnHotspotSelect, new EventManager.Delegate_ChangeHotspot(OnSelectHotspot));
			EventManager.OnHotspotDeselect = (EventManager.Delegate_ChangeHotspot)Delegate.Combine(EventManager.OnHotspotDeselect, new EventManager.Delegate_ChangeHotspot(OnDeselectHotspot));
			EventManager.OnMouseOverMenu = (EventManager.Delegate_OnMouseOverMenu)Delegate.Combine(EventManager.OnMouseOverMenu, new EventManager.Delegate_OnMouseOverMenu(OnMouseOverMenu));
			EventManager.OnMenuTurnOff = (EventManager.Delegate_OnMenuTurnOn)Delegate.Combine(EventManager.OnMenuTurnOff, new EventManager.Delegate_OnMenuTurnOn(OnMenuTurnOff));
		}

		private void Update()
		{
			if (hotspotHoverDuration > 0f)
			{
				hotspotHoverDuration -= Time.fixedDeltaTime;
				if (hotspotHoverDuration <= 0f)
				{
					hotspotHoverDuration = 0f;
					if (hoverHotspot != null && KickStarter.playerInteraction.GetActiveHotspot() == hoverHotspot)
					{
						KickStarter.playerInput.SimulateInputButton("InteractionA");
						elementOverIdentifier = string.Empty;
					}
				}
			}
			if (!(menuHoverDuration > 0f))
			{
				return;
			}
			menuHoverDuration -= Time.fixedDeltaTime;
			if (menuHoverDuration <= 0f)
			{
				menuHoverDuration = 0f;
				if (hoverElement != null && hoverMenu != null && hoverMenu.IsPointerOverSlot(hoverElement, hoverSlot, KickStarter.playerInput.GetInvertedMouse()))
				{
					hoverElement.ProcessClick(hoverMenu, hoverSlot, MouseState.SingleClick);
					elementOverIdentifier = string.Empty;
				}
			}
		}

		private void OnSelectHotspot(Hotspot hotspot)
		{
			hoverHotspot = hotspot;
			hotspotHoverDuration = hoverDuration;
		}

		private void OnDeselectHotspot(Hotspot hotspot)
		{
			hoverHotspot = null;
			hotspotHoverDuration = 0f;
		}

		private void OnMouseOverMenu(Menu _menu, MenuElement _element, int _slot)
		{
			string text = string.Empty;
			if (_element != null)
			{
				text = _menu.id + " " + _element.ID + " " + _slot;
			}
			if (text != string.Empty && text != elementOverIdentifier)
			{
				hotspotHoverDuration = 0f;
				menuHoverDuration = hoverDuration;
				hoverMenu = _menu;
				hoverElement = _element;
				hoverSlot = _slot;
			}
			elementOverIdentifier = text;
		}

		private void OnMenuTurnOff(Menu _menu, bool isInstant)
		{
			if (_menu.title == "Interaction" && hoverHotspot != null && KickStarter.playerInteraction.GetActiveHotspot() == hoverHotspot)
			{
				OnSelectHotspot(hoverHotspot);
			}
		}

		private void OnDisable()
		{
			EventManager.OnHotspotSelect = (EventManager.Delegate_ChangeHotspot)Delegate.Remove(EventManager.OnHotspotSelect, new EventManager.Delegate_ChangeHotspot(OnSelectHotspot));
			EventManager.OnHotspotDeselect = (EventManager.Delegate_ChangeHotspot)Delegate.Remove(EventManager.OnHotspotDeselect, new EventManager.Delegate_ChangeHotspot(OnDeselectHotspot));
			EventManager.OnMouseOverMenu = (EventManager.Delegate_OnMouseOverMenu)Delegate.Remove(EventManager.OnMouseOverMenu, new EventManager.Delegate_OnMouseOverMenu(OnMouseOverMenu));
			EventManager.OnMenuTurnOff = (EventManager.Delegate_OnMenuTurnOn)Delegate.Remove(EventManager.OnMenuTurnOff, new EventManager.Delegate_OnMenuTurnOn(OnMenuTurnOff));
		}
	}
}
