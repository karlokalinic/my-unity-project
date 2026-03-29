using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AC
{
	public class NavigationEngine_UnityNavigation : NavigationEngine
	{
		public override void SceneSettingsGUI()
		{
		}

		public override void TurnOn(NavigationMesh navMesh)
		{
			ACDebug.LogWarning("Cannot enable NavMesh " + navMesh.gameObject.name + " as this scene's Navigation Method is Unity Navigation.");
		}

		public override Vector3[] GetPointsArray(Vector3 startPosition, Vector3 targetPosition, Char _char = null)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			if (!NavMesh.CalculatePath(startPosition, targetPosition, -1, navMeshPath))
			{
				float num = 0.001f;
				float num2 = Vector3.Distance(startPosition, targetPosition);
				NavMeshHit hit = default(NavMeshHit);
				for (num = 0.001f; num < num2; num += 0.05f)
				{
					if (NavMesh.SamplePosition(startPosition, out hit, num, -1))
					{
						startPosition = hit.position;
						break;
					}
				}
				bool flag = false;
				for (num = 0.001f; num < num2; num += 0.05f)
				{
					if (NavMesh.SamplePosition(targetPosition, out hit, num, -1))
					{
						targetPosition = hit.position;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return new Vector3[0];
				}
				NavMesh.CalculatePath(startPosition, targetPosition, -1, navMeshPath);
			}
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < navMeshPath.corners.Length; i++)
			{
				list.Add(navMeshPath.corners[i]);
			}
			if (list.Count > 1 && Mathf.Approximately(list[0].x, startPosition.x) && Mathf.Approximately(list[0].z, startPosition.x))
			{
				list.RemoveAt(0);
			}
			else if (list.Count == 0)
			{
				list.Clear();
				list.Add(targetPosition);
			}
			return list.ToArray();
		}

		public override Vector3 GetPointNear(Vector3 point, float minDistance, float maxDistance)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector3 vector = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * Random.Range(minDistance, maxDistance);
			Vector3 vector2 = point + vector;
			NavMeshHit hit = default(NavMeshHit);
			if (!NavMesh.Raycast(point, vector2, out hit, -1))
			{
				return vector2;
			}
			if (hit.position != Vector3.zero)
			{
				return hit.position;
			}
			return base.GetPointNear(point, minDistance, maxDistance);
		}

		public override string GetPrefabName()
		{
			return "NavMeshSegment";
		}

		public override void SetVisibility(bool visibility)
		{
			NavMeshSegment[] array = Object.FindObjectsOfType(typeof(NavMeshSegment)) as NavMeshSegment[];
			NavMeshSegment[] array2 = array;
			foreach (NavMeshSegment navMeshSegment in array2)
			{
				if (visibility)
				{
					navMeshSegment.Show();
				}
				else
				{
					navMeshSegment.Hide();
				}
			}
		}
	}
}
