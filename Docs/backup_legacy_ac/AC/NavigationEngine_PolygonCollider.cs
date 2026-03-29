using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class NavigationEngine_PolygonCollider : NavigationEngine
	{
		public static Collider2D[] results = new Collider2D[1];

		protected int MAXNODES = 1000;

		protected List<float[,]> allCachedGraphs = new List<float[,]>();

		protected float searchRadius = 0.02f;

		protected Vector2 dir_n = new Vector2(0f, 1f);

		protected Vector2 dir_s = new Vector2(0f, -1f);

		protected Vector2 dir_w = new Vector2(-1f, 0f);

		protected Vector2 dir_e = new Vector2(1f, 0f);

		protected Vector2 dir_ne = new Vector2(0.71f, 0.71f);

		protected Vector2 dir_se = new Vector2(0.71f, -0.71f);

		protected Vector2 dir_sw = new Vector2(-0.71f, -0.71f);

		protected Vector2 dir_nw = new Vector2(-0.71f, 0.71f);

		protected Vector2 dir_nne = new Vector2(0.37f, 0.93f);

		protected Vector2 dir_nee = new Vector2(0.93f, 0.37f);

		protected Vector2 dir_see = new Vector2(0.93f, -0.37f);

		protected Vector2 dir_sse = new Vector2(0.37f, -0.93f);

		protected Vector2 dir_ssw = new Vector2(-0.37f, -0.93f);

		protected Vector2 dir_sww = new Vector2(-0.93f, -0.37f);

		protected Vector2 dir_nww = new Vector2(-0.93f, 0.37f);

		protected Vector2 dir_nnw = new Vector2(-0.37f, 0.93f);

		protected List<Vector2[]> allVertexData = new List<Vector2[]>();

		public override void OnReset(NavigationMesh navMesh)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			is2D = true;
			ResetHoles(navMesh);
			if (navMesh != null && navMesh.characterEvasion != CharacterEvasion.None && navMesh.GetComponent<PolygonCollider2D>() != null)
			{
				PolygonCollider2D[] polygonCollider2Ds = navMesh.PolygonCollider2Ds;
				if (polygonCollider2Ds != null && polygonCollider2Ds.Length > 1)
				{
					ACDebug.LogWarning("Character evasion cannot occur for multiple PolygonColliders - only the first on the active NavMesh will be affected.");
				}
				for (int i = 0; i < polygonCollider2Ds.Length; i++)
				{
					if (!polygonCollider2Ds[i].isTrigger)
					{
						ACDebug.LogWarning("The PolygonCollider2D on " + navMesh.gameObject.name + " is not a Trigger.", navMesh.gameObject);
					}
					if (polygonCollider2Ds[i].offset != Vector2.zero)
					{
						ACDebug.LogWarning("The PolygonCollider2D on " + navMesh.gameObject.name + " has a non-zero Offset - this can cause pathfinding errors.  Clear this offset and adjust the GameObject's position if necessary.", navMesh.gameObject);
					}
				}
			}
			if (navMesh == null && KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick)
			{
				ACDebug.LogWarning("Could not initialise NavMesh - was one set as the Default in the Scene Manager?");
			}
		}

		public override void TurnOn(NavigationMesh navMesh)
		{
			if (!(navMesh == null) && !(KickStarter.settingsManager == null))
			{
				if (LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer) == -1)
				{
					ACDebug.LogError("Can't find layer " + KickStarter.settingsManager.navMeshLayer + " - please define it in Unity's Tags Manager (Edit -> Project settings -> Tags and Layers).");
				}
				else if (!string.IsNullOrEmpty(KickStarter.settingsManager.navMeshLayer))
				{
					navMesh.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer);
				}
				if (navMesh.GetComponent<Collider2D>() == null)
				{
					ACDebug.LogWarning("A 2D Collider component must be attached to " + navMesh.gameObject.name + " for pathfinding to work - please attach one.");
				}
			}
		}

		public override Vector3[] GetPointsArray(Vector3 _originPos, Vector3 _targetPos, Char _char = null)
		{
			if (KickStarter.sceneSettings == null || KickStarter.sceneSettings.navMesh == null)
			{
				return base.GetPointsArray(_originPos, _targetPos, _char);
			}
			PolygonCollider2D[] polygonCollider2Ds = KickStarter.sceneSettings.navMesh.PolygonCollider2Ds;
			if (polygonCollider2Ds == null || polygonCollider2Ds.Length == 0)
			{
				return base.GetPointsArray(_originPos, _targetPos, _char);
			}
			CalcSearchRadius(KickStarter.sceneSettings.navMesh);
			AddCharHoles(polygonCollider2Ds, _char, KickStarter.sceneSettings.navMesh);
			List<Vector3> list = new List<Vector3>();
			if (IsLineClear(_originPos, _targetPos))
			{
				list.Add(_targetPos);
				return list.ToArray();
			}
			int num = -1;
			float num2 = 0f;
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = new Vector2(_originPos.x, _originPos.y);
			for (int i = 0; i < polygonCollider2Ds.Length; i++)
			{
				Vector2 nearestToMesh = GetNearestToMesh(_originPos, polygonCollider2Ds[i], polygonCollider2Ds.Length > 1);
				float sqrMagnitude = (vector2 - nearestToMesh).sqrMagnitude;
				if (num < 0 || sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					vector = nearestToMesh;
					num = i;
				}
			}
			if (num < 0)
			{
				num = 0;
			}
			Vector2 nearestToMesh2 = GetNearestToMesh(_targetPos, polygonCollider2Ds[num], polygonCollider2Ds.Length > 1);
			Vector2[] points = allVertexData[num];
			points = AddEndsToList(points, vector, nearestToMesh2);
			bool useCache = KickStarter.sceneSettings.navMesh.characterEvasion == CharacterEvasion.None;
			float[,] weight = pointsToWeight(points, useCache, num);
			int[] array = buildSpanningTree(0, 1, weight);
			if (array == null)
			{
				ACDebug.LogWarning(string.Concat("Pathfinding error - cannot build spanning tree from ", vector, " to ", nearestToMesh2));
				list.Add(_targetPos);
				return list.ToArray();
			}
			int[] shortestPath = getShortestPath(0, 1, array);
			int[] array2 = shortestPath;
			foreach (int num3 in array2)
			{
				if (num3 < points.Length)
				{
					Vector3 item = new Vector3(points[num3].x, points[num3].y, _originPos.z);
					list.Insert(0, item);
				}
			}
			if (list.Count > 1)
			{
				if (list[0] == _originPos || (Mathf.Approximately(list[0].x, vector.x) && Mathf.Approximately(list[0].y, vector.y)))
				{
					list.RemoveAt(0);
				}
			}
			else if (list.Count == 0)
			{
				ACDebug.LogError(string.Concat("Error attempting to pathfind to point ", _targetPos, " corrected = ", nearestToMesh2));
				list.Add(vector);
			}
			return list.ToArray();
		}

		public override void ResetHoles(NavigationMesh navMesh)
		{
			ResetHoles(navMesh, true);
		}

		protected void ResetHoles(NavigationMesh navMesh, bool rebuild)
		{
			if (navMesh == null)
			{
				return;
			}
			CalcSearchRadius(navMesh);
			PolygonCollider2D[] polygonCollider2Ds = navMesh.PolygonCollider2Ds;
			if (polygonCollider2Ds == null || polygonCollider2Ds.Length == 0)
			{
				return;
			}
			for (int i = 0; i < polygonCollider2Ds.Length; i++)
			{
				polygonCollider2Ds[i].pathCount = 1;
				if (i > 0 || navMesh.polygonColliderHoles.Count == 0)
				{
					if (rebuild)
					{
						RebuildVertexArray(navMesh.transform, polygonCollider2Ds[i], i);
						CreateCache(i);
					}
					continue;
				}
				Vector2 vector = new Vector2(1f / navMesh.transform.localScale.x, 1f / navMesh.transform.localScale.y);
				foreach (PolygonCollider2D polygonColliderHole in navMesh.polygonColliderHoles)
				{
					if (polygonColliderHole != null)
					{
						polygonCollider2Ds[i].pathCount++;
						List<Vector2> list = new List<Vector2>();
						Vector2[] points = polygonColliderHole.points;
						foreach (Vector2 vector2 in points)
						{
							Vector2 vector3 = polygonColliderHole.transform.TransformPoint(vector2) - navMesh.transform.position;
							list.Add(new Vector2(vector3.x * vector.x, vector3.y * vector.y));
						}
						polygonCollider2Ds[i].SetPath(polygonCollider2Ds[i].pathCount - 1, list.ToArray());
						polygonColliderHole.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
						polygonColliderHole.isTrigger = true;
					}
				}
				if (rebuild)
				{
					RebuildVertexArray(navMesh.transform, polygonCollider2Ds[i], i);
					CreateCache(i);
				}
			}
		}

		public override Vector3 GetPointNear(Vector3 point, float minDistance, float maxDistance)
		{
			Vector2 vector = Random.insideUnitCircle * Random.Range(minDistance, maxDistance);
			Vector2 vector2 = (Vector2)point + vector;
			if (IsLineClear(point, vector2))
			{
				return vector2;
			}
			Vector2 lineIntersect = GetLineIntersect(vector2, point);
			if (lineIntersect != Vector2.zero)
			{
				return lineIntersect;
			}
			return base.GetPointNear(point, minDistance, maxDistance);
		}

		protected int[] buildSpanningTree(int source, int destination, float[,] weight)
		{
			int num = (int)Mathf.Sqrt(weight.Length);
			bool[] array = new bool[num];
			float[] array2 = new float[num];
			int[] array3 = new int[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = float.PositiveInfinity;
				array3[i] = 100000;
			}
			array2[source] = 0f;
			int num2 = source;
			while (num2 != destination)
			{
				if (num2 < 0)
				{
					return null;
				}
				float num3 = array2[num2];
				float num4 = float.PositiveInfinity;
				int num5 = -1;
				array[num2] = true;
				for (int j = 0; j < num; j++)
				{
					if (!array[j])
					{
						float num6 = ((!Mathf.Approximately(weight[num2, j], -1f)) ? (num3 + weight[num2, j]) : float.PositiveInfinity);
						if (num6 < array2[j])
						{
							array2[j] = num6;
							array3[j] = num2;
						}
						if (array2[j] < num4)
						{
							num4 = array2[j];
							num5 = j;
						}
					}
				}
				num2 = num5;
			}
			return array3;
		}

		protected int[] getShortestPath(int source, int destination, int[] precede)
		{
			int num = destination;
			int num2 = 0;
			int[] array = new int[MAXNODES];
			array[num2] = destination;
			num2++;
			while (precede[num] != source)
			{
				num = (array[num2] = precede[num]);
				num2++;
			}
			array[num2] = source;
			int[] array2 = new int[num2 + 1];
			for (int i = 0; i < num2 + 1; i++)
			{
				array2[i] = array[i];
			}
			return array2;
		}

		protected float[,] pointsToWeight(Vector2[] points, bool useCache = false, int polyIndex = 0)
		{
			int num = points.Length;
			int num2 = num;
			float[,] array = new float[num, num];
			if (useCache)
			{
				array = allCachedGraphs[polyIndex];
				num = 2;
			}
			for (int i = 0; i < num; i++)
			{
				for (int j = i; j < num2; j++)
				{
					if (i == j)
					{
						array[i, j] = -1f;
					}
					else if (!IsLineClear(points[i], points[j]))
					{
						array[i, j] = (array[j, i] = -1f);
					}
					else
					{
						array[i, j] = (array[j, i] = (points[i] - points[j]).magnitude);
					}
				}
			}
			return array;
		}

		protected Vector2 GetNearestToMesh(Vector2 vertex, PolygonCollider2D poly, bool hasMultiple)
		{
			RaycastHit2D raycastHit2D = UnityVersionHandler.Perform2DRaycast(vertex - new Vector2(0.005f, 0f), new Vector2(1f, 0f), 0.01f, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer);
			if (!raycastHit2D)
			{
				raycastHit2D = UnityVersionHandler.Perform2DRaycast(vertex - new Vector2(0f, 0.005f), new Vector2(0f, 1f), 0.01f, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer);
			}
			if (!raycastHit2D)
			{
				return GetNearestOffMesh(vertex, poly);
			}
			if (hasMultiple && raycastHit2D.collider != null && raycastHit2D.collider is PolygonCollider2D && raycastHit2D.collider != poly)
			{
				return GetNearestOffMesh(vertex, poly);
			}
			return vertex;
		}

		protected Vector2 GetNearestOffMesh(Vector2 vertex, PolygonCollider2D poly)
		{
			Transform transform = KickStarter.sceneSettings.navMesh.transform;
			float num = -1f;
			Vector2 result = vertex;
			Vector2 vector = vertex;
			for (int i = 0; i < poly.pathCount; i++)
			{
				Vector2[] path = poly.GetPath(i);
				for (int j = 0; j < path.Length; j++)
				{
					Vector2 vector2 = transform.TransformPoint(path[j]);
					Vector2 vector3 = ((j >= path.Length - 1) ? transform.TransformPoint(path[0]) : transform.TransformPoint(path[j + 1]));
					Vector2 vector4 = vector3 - vector2;
					for (float num2 = 0f; num2 <= 1f; num2 += 0.1f)
					{
						vector = vector2 + vector4 * num2;
						float sqrMagnitude = (vertex - vector).sqrMagnitude;
						if (sqrMagnitude < num || num < 0f)
						{
							num = sqrMagnitude;
							result = vector;
						}
					}
				}
			}
			return result;
		}

		protected Vector2[] AddEndsToList(Vector2[] points, Vector2 originPos, Vector2 targetPos, bool checkForDuplicates = true)
		{
			List<Vector2> list = new List<Vector2>();
			foreach (Vector2 vector in points)
			{
				if ((vector != originPos && vector != targetPos) || !checkForDuplicates)
				{
					list.Add(vector);
				}
			}
			list.Insert(0, targetPos);
			list.Insert(0, originPos);
			return list.ToArray();
		}

		protected bool IsLineClear(Vector2 startPos, Vector2 endPos)
		{
			Vector2 vector = startPos;
			Vector2 normalized = (endPos - startPos).normalized;
			float magnitude = (endPos - startPos).magnitude;
			float num = 100f * searchRadius * searchRadius;
			for (float num2 = 0f; num2 < magnitude; num2 += num)
			{
				vector = startPos + normalized * num2;
				if (Perform2DOverlapCircle(vector, searchRadius, results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 1)
				{
					return false;
				}
			}
			return true;
		}

		protected Vector2 GetLineIntersect(Vector2 startPos, Vector2 endPos)
		{
			Vector2 vector = startPos;
			Vector2 normalized = (endPos - startPos).normalized;
			float magnitude = (endPos - startPos).magnitude;
			int num = 0;
			float num2 = magnitude * 0.02f;
			for (float num3 = 0f; num3 < magnitude; num3 += num2 * 2f)
			{
				vector = startPos + normalized * num3;
				if (Perform2DOverlapCircle(vector, num2, results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
				{
					num++;
				}
				if (num == 2)
				{
					return vector;
				}
			}
			return Vector2.zero;
		}

		public override string GetPrefabName()
		{
			return "NavMesh2D";
		}

		public override void SetVisibility(bool visibility)
		{
		}

		public override void SceneSettingsGUI()
		{
		}

		protected void AddCharHoles(PolygonCollider2D[] navPolys, Char charToExclude, NavigationMesh navigationMesh)
		{
			if (navigationMesh.characterEvasion == CharacterEvasion.None)
			{
				return;
			}
			ResetHoles(KickStarter.sceneSettings.navMesh, false);
			for (int i = 0; i < navPolys.Length && i <= 0; i++)
			{
				if (navPolys[i].transform.lossyScale != Vector3.one)
				{
					ACDebug.LogWarning("Cannot create evasion Polygons inside NavMesh '" + navPolys[i].gameObject.name + "' because it has a non-unit scale.");
					continue;
				}
				Vector2 vector = navPolys[i].transform.position;
				for (int j = 0; j < KickStarter.stateHandler.Characters.Count; j++)
				{
					Char obj = KickStarter.stateHandler.Characters[j];
					CircleCollider2D component = obj.GetComponent<CircleCollider2D>();
					if (!(component != null) || (obj.charState != CharState.Idle && navigationMesh.characterEvasion != CharacterEvasion.AllCharacters) || (!(charToExclude == null) && !(obj != charToExclude)) || Perform2DOverlapPoint(obj.transform.position, results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) == 0)
					{
						continue;
					}
					if (!obj.IsPlayer || KickStarter.settingsManager.movementMethod != MovementMethod.Direct)
					{
						component.isTrigger = true;
					}
					List<Vector2> list = new List<Vector2>();
					Vector2 vector2 = obj.transform.TransformPoint(component.offset);
					float num = component.radius * obj.transform.localScale.x;
					float characterEvasionYScale = navigationMesh.characterEvasionYScale;
					switch (navigationMesh.characterEvasionPoints)
					{
					case CharacterEvasionPoints.Four:
						list.Add(vector2 + new Vector2(dir_n.x * num, dir_n.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_e.x * num, dir_e.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_s.x * num, dir_s.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_w.x * num, dir_w.y * num * characterEvasionYScale));
						break;
					case CharacterEvasionPoints.Eight:
						list.Add(vector2 + new Vector2(dir_n.x * num, dir_n.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_ne.x * num, dir_ne.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_e.x * num, dir_e.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_se.x * num, dir_se.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_s.x * num, dir_s.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_sw.x * num, dir_sw.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_w.x * num, dir_w.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nw.x * num, dir_nw.y * num * characterEvasionYScale));
						break;
					case CharacterEvasionPoints.Sixteen:
						list.Add(vector2 + new Vector2(dir_n.x * num, dir_n.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nne.x * num, dir_nne.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_ne.x * num, dir_ne.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nee.x * num, dir_nee.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_e.x * num, dir_e.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_see.x * num, dir_see.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_se.x * num, dir_se.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_sse.x * num, dir_sse.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_s.x * num, dir_s.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_ssw.x * num, dir_ssw.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_sw.x * num, dir_sw.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_sww.x * num, dir_sww.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_w.x * num, dir_w.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nww.x * num, dir_nww.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nw.x * num, dir_nw.y * num * characterEvasionYScale));
						list.Add(vector2 + new Vector2(dir_nnw.x * num, dir_nnw.y * num * characterEvasionYScale));
						break;
					}
					navPolys[i].pathCount++;
					List<Vector2> list2 = new List<Vector2>();
					for (int k = 0; k < list.Count; k++)
					{
						if (Perform2DOverlapPoint(list[k], results, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) != 0)
						{
							list2.Add(list[k] - vector);
							continue;
						}
						Vector2 lineIntersect = GetLineIntersect(list[k], vector2);
						if (lineIntersect != Vector2.zero)
						{
							list2.Add(lineIntersect - vector);
						}
					}
					if (list2.Count > 1)
					{
						navPolys[i].SetPath(navPolys[i].pathCount - 1, list2.ToArray());
					}
				}
				RebuildVertexArray(navPolys[i].transform, navPolys[i], i);
			}
		}

		protected void RebuildVertexArray(Transform navMeshTransform, PolygonCollider2D poly, int polyIndex)
		{
			if (allVertexData == null)
			{
				allVertexData = new List<Vector2[]>();
			}
			if (allVertexData.Count <= polyIndex)
			{
				while (allVertexData.Count <= polyIndex)
				{
					allVertexData.Add(new Vector2[0]);
				}
			}
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i < poly.pathCount; i++)
			{
				Vector2[] path = poly.GetPath(i);
				for (int j = 0; j < path.Length; j++)
				{
					Vector3 vector = navMeshTransform.TransformPoint(new Vector3(path[j].x, path[j].y, navMeshTransform.position.z));
					list.Add(new Vector2(vector.x, vector.y));
				}
			}
			allVertexData[polyIndex] = list.ToArray();
		}

		protected void CalcSearchRadius(NavigationMesh navMesh)
		{
			searchRadius = 0.1f - 0.08f * navMesh.accuracy;
		}

		protected void CreateCache(int i)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			Vector2[] points = allVertexData[i];
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			points = AddEndsToList(points, zero, zero2, false);
			if (allCachedGraphs == null)
			{
				allCachedGraphs = new List<float[,]>();
			}
			if (allCachedGraphs.Count <= i)
			{
				while (allCachedGraphs.Count <= i)
				{
					allCachedGraphs.Add(new float[0, 0]);
				}
			}
			allCachedGraphs[i] = pointsToWeight(points, false, i);
		}

		private int Perform2DOverlapCircle(Vector2 point, float radius, Collider2D[] results, LayerMask layerMask)
		{
			ContactFilter2D contactFilter = default(ContactFilter2D);
			contactFilter.useTriggers = true;
			contactFilter.SetLayerMask(layerMask);
			contactFilter.ClearDepth();
			return Physics2D.OverlapCircle(point, radius, contactFilter, results);
		}

		private int Perform2DOverlapPoint(Vector2 point, Collider2D[] results, LayerMask layerMask)
		{
			ContactFilter2D contactFilter = default(ContactFilter2D);
			contactFilter.useTriggers = true;
			contactFilter.SetLayerMask(layerMask);
			contactFilter.ClearDepth();
			return Physics2D.OverlapPoint(point, contactFilter, results);
		}
	}
}
