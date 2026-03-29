using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_menu_link.html")]
	public class MenuLink : MonoBehaviour
	{
		public string menuName = string.Empty;

		public string elementName = string.Empty;

		public int slot;

		public bool setTextLabels;

		protected Menu menu;

		protected MenuElement element;

		protected TextMesh textMesh;

		protected void Start()
		{
			if (string.IsNullOrEmpty(menuName) || string.IsNullOrEmpty(elementName))
			{
				return;
			}
			textMesh = GetComponent<TextMesh>();
			try
			{
				menu = PlayerMenus.GetMenuWithName(menuName);
				element = PlayerMenus.GetElementWithName(menuName, elementName);
			}
			catch
			{
				ACDebug.LogWarning("Cannot find Menu Element with name: " + elementName + " on Menu: " + menuName);
			}
		}

		protected void FixedUpdate()
		{
			if ((bool)element && setTextLabels)
			{
				int language = Options.GetLanguage();
				if (textMesh != null)
				{
					textMesh.text = GetLabel(language);
				}
			}
		}

		protected void OnDestroy()
		{
			element = null;
			menu = null;
		}

		public string GetLabel(int languageNumber)
		{
			if ((bool)element)
			{
				return element.GetLabel(slot, languageNumber);
			}
			return string.Empty;
		}

		public bool IsVisible()
		{
			if ((bool)element && (bool)menu)
			{
				if (!menu.IsVisible())
				{
					return false;
				}
				return element.IsVisible;
			}
			return false;
		}

		public void Interact()
		{
			if ((bool)element)
			{
				if (!element.isClickable)
				{
					ACDebug.Log("Cannot click on " + elementName);
				}
				PlayerMenus.SimulateClick(menuName, element, slot);
			}
		}
	}
}
