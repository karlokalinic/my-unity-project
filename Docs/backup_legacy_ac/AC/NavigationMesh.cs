using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_navigation_mesh.html")]
	public class NavigationMesh : NavMeshBase
	{
		public List<PolygonCollider2D> polygonColliderHoles = new List<PolygonCollider2D>();

		public CharacterEvasion characterEvasion = CharacterEvasion.OnlyStationaryCharacters;

		public CharacterEvasionPoints characterEvasionPoints;

		public float characterEvasionYScale = 1f;

		public float accuracy = 1f;

		public Color gizmoColour = Color.white;

		protected PolygonCollider2D[] polygonCollider2Ds;

		public PolygonCollider2D[] PolygonCollider2Ds
		{
			get
			{
				if (polygonCollider2Ds == null)
				{
					polygonCollider2Ds = GetComponents<PolygonCollider2D>();
				}
				return polygonCollider2Ds;
			}
		}

		protected void Awake()
		{
			BaseAwake();
		}

		public void AddHole(PolygonCollider2D newHole)
		{
			if (!polygonColliderHoles.Contains(newHole))
			{
				polygonColliderHoles.Add(newHole);
				ResetHoles();
				if (GetComponent<RememberNavMesh2D>() == null)
				{
					ACDebug.LogWarning("Changes to " + base.gameObject.name + "'s holes will not be saved because it has no RememberNavMesh2D script", base.gameObject);
				}
			}
		}

		public void RemoveHole(PolygonCollider2D oldHole)
		{
			if (polygonColliderHoles.Contains(oldHole))
			{
				polygonColliderHoles.Remove(oldHole);
				ResetHoles();
			}
		}

		public void TurnOn()
		{
			if ((bool)KickStarter.navigationManager.navigationEngine)
			{
				KickStarter.navigationManager.navigationEngine.TurnOn(this);
				KickStarter.navigationManager.navigationEngine.ResetHoles(this);
			}
		}

		public void TurnOff()
		{
			if (KickStarter.settingsManager != null)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
		}

		private void ResetHoles()
		{
			KickStarter.navigationManager.navigationEngine.ResetHoles(this);
		}
	}
}
