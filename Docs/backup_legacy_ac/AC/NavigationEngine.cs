using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class NavigationEngine : ScriptableObject
	{
		public bool is2D;

		protected Vector2[] vertexData;

		public virtual void OnReset(NavigationMesh navMesh)
		{
		}

		public virtual Vector3[] GetPointsArray(Vector3 startPosition, Vector3 targetPosition, Char _char = null)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(targetPosition);
			return list.ToArray();
		}

		public Vector3[] GetPointsArray(Vector3 startPosition, Vector3[] targetPositions, Char _char = null)
		{
			if (targetPositions == null || targetPositions.Length == 0)
			{
				return GetPointsArray(startPosition, startPosition, _char);
			}
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < targetPositions.Length; i++)
			{
				Vector3 startPosition2 = ((i <= 0) ? startPosition : targetPositions[i - 1]);
				Vector3[] pointsArray = GetPointsArray(startPosition2, targetPositions[i], _char);
				Vector3[] array = pointsArray;
				foreach (Vector3 vector in array)
				{
					if (list.Count == 0 || list[list.Count - 1] != vector)
					{
						list.Add(vector);
					}
				}
			}
			return list.ToArray();
		}

		public virtual Vector3 GetPointNear(Vector3 point, float minDistance, float maxDistance)
		{
			return point;
		}

		public virtual string GetPrefabName()
		{
			return string.Empty;
		}

		public virtual void TurnOn(NavigationMesh navMesh)
		{
		}

		public virtual void SetVisibility(bool visibility)
		{
		}

		public virtual void ResetHoles(NavigationMesh navMesh)
		{
		}

		public virtual void SceneSettingsGUI()
		{
		}
	}
}
