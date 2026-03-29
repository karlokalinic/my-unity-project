using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{
	public class UISlotClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		private Menu menu;

		private MenuElement menuElement;

		private int slot;

		public void Setup(Menu _menu, MenuElement _element, int _slot)
		{
			if (!(_menu == null))
			{
				menu = _menu;
				menuElement = _element;
				slot = _slot;
			}
		}

		private void Update()
		{
			if ((bool)menuElement && KickStarter.playerInput != null && KickStarter.playerInput.InputGetButtonDown("InteractionB") && KickStarter.playerMenus.IsEventSystemSelectingObject(base.gameObject))
			{
				menuElement.ProcessClick(menu, slot, MouseState.RightClick);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if ((bool)menuElement && eventData.button == PointerEventData.InputButton.Right)
			{
				menuElement.ProcessClick(menu, slot, MouseState.RightClick);
			}
		}
	}
}
