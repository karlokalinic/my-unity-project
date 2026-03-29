using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_nav_mesh_segment.html")]
	[AddComponentMenu("Adventure Creator/Navigation/NavMesh Segment")]
	public class NavMeshSegment : NavMeshBase
	{
		protected void Awake()
		{
			BaseAwake();
			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.UnityNavigation)
			{
				if (LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer) == -1)
				{
					ACDebug.LogWarning("No 'NavMesh' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer);
				}
			}
		}
	}
}
