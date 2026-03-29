using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_menu_system.html")]
	public class MenuSystem : MonoBehaviour
	{
		public static void OnMenuEnable(Menu _menu)
		{
			if (_menu.title == "Pause")
			{
				MenuElement elementWithName = _menu.GetElementWithName("SaveButton");
				if ((bool)elementWithName)
				{
					elementWithName.IsVisible = !PlayerMenus.IsSavingLocked();
				}
				_menu.Recalculate();
			}
		}

		public static void OnElementClick(Menu _menu, MenuElement _element, int _slot, int _buttonPressed)
		{
		}
	}
}
