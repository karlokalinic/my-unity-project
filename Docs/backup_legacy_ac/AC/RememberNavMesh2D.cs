using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember NavMesh2D")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_nav_mesh2_d.html")]
	public class RememberNavMesh2D : Remember
	{
		public override string SaveData()
		{
			NavMesh2DData navMesh2DData = new NavMesh2DData();
			navMesh2DData.objectID = constantID;
			navMesh2DData.savePrevented = savePrevented;
			if ((bool)GetComponent<NavigationMesh>())
			{
				NavigationMesh component = GetComponent<NavigationMesh>();
				List<int> list = new List<int>();
				for (int i = 0; i < component.polygonColliderHoles.Count; i++)
				{
					if ((bool)component.polygonColliderHoles[i].GetComponent<ConstantID>())
					{
						list.Add(component.polygonColliderHoles[i].GetComponent<ConstantID>().constantID);
						continue;
					}
					ACDebug.LogWarning("Cannot save " + base.gameObject.name + "'s holes because " + component.polygonColliderHoles[i].gameObject.name + " has no Constant ID!", base.gameObject);
				}
				navMesh2DData._linkedIDs = ArrayToString(list.ToArray());
			}
			return Serializer.SaveScriptData<NavMesh2DData>(navMesh2DData);
		}

		public override void LoadData(string stringData)
		{
			NavMesh2DData navMesh2DData = Serializer.LoadScriptData<NavMesh2DData>(stringData);
			if (navMesh2DData == null)
			{
				return;
			}
			base.SavePrevented = navMesh2DData.savePrevented;
			if (savePrevented || !GetComponent<NavigationMesh>())
			{
				return;
			}
			NavigationMesh component = GetComponent<NavigationMesh>();
			component.polygonColliderHoles.Clear();
			KickStarter.navigationManager.navigationEngine.ResetHoles(component);
			if (string.IsNullOrEmpty(navMesh2DData._linkedIDs))
			{
				return;
			}
			int[] array = StringToIntArray(navMesh2DData._linkedIDs);
			for (int i = 0; i < array.Length; i++)
			{
				PolygonCollider2D polygonCollider2D = Serializer.returnComponent<PolygonCollider2D>(array[i]);
				if (polygonCollider2D != null)
				{
					component.AddHole(polygonCollider2D);
				}
			}
		}
	}
}
