using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{
	[Serializable]
	public class MenuManager : ScriptableObject
	{
		public EventSystem eventSystem;

		public List<Menu> menus = new List<Menu>();

		public int globalDepth;

		public Texture2D pauseTexture;

		public bool scaleTextEffects;

		public bool keyboardControlWhenPaused = true;

		public bool keyboardControlWhenDialogOptions = true;

		public bool autoSelectValidRaycasts;

		[SerializeField]
		private bool hasUpgraded;

		public void Upgrade()
		{
			if (KickStarter.settingsManager != null && !hasUpgraded)
			{
				keyboardControlWhenPaused = KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController;
				keyboardControlWhenDialogOptions = KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController;
				hasUpgraded = true;
			}
		}
	}
}
