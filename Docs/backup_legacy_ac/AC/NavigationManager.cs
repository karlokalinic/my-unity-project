using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_navigation_manager.html")]
	public class NavigationManager : MonoBehaviour
	{
		[HideInInspector]
		public NavigationEngine navigationEngine;

		public void OnAwake()
		{
			navigationEngine = null;
			ResetEngine();
		}

		public void ResetEngine()
		{
			string empty = string.Empty;
			empty = ((KickStarter.sceneSettings.navigationMethod != AC_NavigationMethod.Custom) ? ("NavigationEngine_" + KickStarter.sceneSettings.navigationMethod) : KickStarter.sceneSettings.customNavigationClass);
			if (string.IsNullOrEmpty(empty) && Application.isPlaying)
			{
				ACDebug.LogWarning("Could not initialise navigation - a custom script must be assigned if the Pathfinding method is set to Custom.");
			}
			else if (navigationEngine == null || !navigationEngine.ToString().Contains(empty))
			{
				navigationEngine = (NavigationEngine)ScriptableObject.CreateInstance(empty);
				if (navigationEngine != null)
				{
					navigationEngine.OnReset(KickStarter.sceneSettings.navMesh);
				}
			}
		}

		public bool Is2D()
		{
			if (navigationEngine != null)
			{
				return navigationEngine.is2D;
			}
			return false;
		}
	}
}
