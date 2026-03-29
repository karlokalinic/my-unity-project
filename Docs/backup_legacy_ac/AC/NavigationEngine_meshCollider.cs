using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class NavigationEngine_meshCollider : NavigationEngine
	{
		protected class NavMeshData
		{
			public Vector3 vertex;

			public float distance;

			public NavMeshData(Vector3 _vertex, Vector3 _target, Transform navObject)
			{
				vertex = navObject.TransformPoint(_vertex);
				distance = Vector3.Distance(vertex, _target);
			}
		}

		protected bool pathFailed;

		public override void OnReset(NavigationMesh navMesh)
		{
			if (Application.isPlaying && navMesh == null && KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick)
			{
				ACDebug.LogWarning("Could not initialise NavMesh - was one set as the Default in the Settings Manager?");
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
				if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider && navMesh.GetComponent<Collider>() == null)
				{
					ACDebug.LogWarning("A Collider component must be attached to " + navMesh.gameObject.name + " for pathfinding to work - please attach one.", navMesh.gameObject);
				}
			}
		}

		public override Vector3[] GetPointsArray(Vector3 originPos, Vector3 targetPos, Char _char = null)
		{
			List<Vector3> list = new List<Vector3>();
			if ((bool)KickStarter.sceneSettings && (bool)KickStarter.sceneSettings.navMesh && (bool)KickStarter.sceneSettings.navMesh.GetComponent<Collider>())
			{
				Vector3 vector = originPos;
				Vector3 value = targetPos;
				originPos = GetNearestToMesh(originPos);
				targetPos = GetNearestToMesh(targetPos);
				list.Add(originPos);
				if (!IsLineClear(targetPos, originPos, false))
				{
					list = FindComplexPath(originPos, targetPos, false);
					if (pathFailed)
					{
						Vector3 lineBreak = GetLineBreak(list[list.Count - 1], targetPos);
						if (lineBreak != Vector3.zero)
						{
							targetPos = lineBreak;
							if (!IsLineClear(targetPos, originPos, true))
							{
								list = FindComplexPath(originPos, targetPos, true);
								if (pathFailed)
								{
									list.Clear();
									list.Add(originPos);
								}
							}
							else
							{
								list.Clear();
								list.Add(originPos);
							}
						}
					}
				}
				if (list.Count > 2)
				{
					for (int i = 0; i < list.Count; i++)
					{
						for (int j = i; j < list.Count; j++)
						{
							if (IsLineClear(list[i], list[j], false) && j > i + 1)
							{
								list.RemoveRange(i + 1, j - i - 1);
								j = 0;
								i = 0;
							}
						}
					}
				}
				list.Add(targetPos);
				if (list[0] == vector)
				{
					list.RemoveAt(0);
				}
				if (list.Count == 1 && list[0] == originPos)
				{
					list[0] = value;
				}
			}
			else
			{
				list.Add(targetPos);
			}
			return list.ToArray();
		}

		public override void ResetHoles(NavigationMesh navMesh)
		{
			if (!(navMesh == null) && !(navMesh.GetComponent<MeshCollider>() == null) && !(navMesh.GetComponent<MeshCollider>().sharedMesh == null) && navMesh.GetComponent<MeshCollider>().sharedMesh == null)
			{
				if ((bool)navMesh.GetComponent<MeshFilter>() && (bool)navMesh.GetComponent<MeshFilter>().sharedMesh)
				{
					navMesh.GetComponent<MeshCollider>().sharedMesh = navMesh.GetComponent<MeshFilter>().sharedMesh;
					ACDebug.LogWarning(navMesh.gameObject.name + " has no MeshCollider mesh - temporarily using MeshFilter mesh instead.", navMesh.gameObject);
				}
				else
				{
					ACDebug.LogWarning(navMesh.gameObject.name + " has no MeshCollider mesh.", navMesh.gameObject);
				}
			}
		}

		public override string GetPrefabName()
		{
			return "NavMesh";
		}

		public override void SetVisibility(bool visibility)
		{
			NavigationMesh[] array = Object.FindObjectsOfType(typeof(NavigationMesh)) as NavigationMesh[];
			NavigationMesh[] array2 = array;
			foreach (NavigationMesh navigationMesh in array2)
			{
				if (visibility)
				{
					navigationMesh.Show();
				}
				else
				{
					navigationMesh.Hide();
				}
			}
		}

		public override Vector3 GetPointNear(Vector3 point, float minDistance, float maxDistance)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector3 vector = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * Random.Range(minDistance, maxDistance);
			Vector3 vector2 = point + vector;
			if (IsLineClear(point, vector2, false))
			{
				return vector2;
			}
			Vector3 lineBreak = GetLineBreak(vector2, point);
			if (lineBreak != Vector3.zero)
			{
				return lineBreak;
			}
			return base.GetPointNear(point, minDistance, maxDistance);
		}

		public override void SceneSettingsGUI()
		{
		}

		protected bool IsVertexImperfect(Vector3 vertex, Vector3[] blackList)
		{
			for (int i = 0; i < blackList.Length; i++)
			{
				if (vertex == blackList[i])
				{
					return true;
				}
			}
			return false;
		}

		protected float GetPathLength(List<Vector3> _pointsList, Vector3 candidatePoint, Vector3 endPos)
		{
			float num = 0f;
			List<Vector3> list = new List<Vector3>();
			foreach (Vector3 _points in _pointsList)
			{
				list.Add(_points);
			}
			list.Add(candidatePoint);
			list.Add(endPos);
			for (int i = 1; i < list.Count; i++)
			{
				num += Vector3.Distance(list[i], list[i - 1]);
			}
			return num;
		}

		protected bool IsLineClear(Vector3 startPos, Vector3 endPos, bool ignoreOthers)
		{
			if (startPos.y > endPos.y)
			{
				endPos.y = startPos.y;
			}
			else
			{
				startPos.y = endPos.y;
			}
			Vector3 vector = startPos;
			RaycastHit hitInfo = default(RaycastHit);
			Ray ray = default(Ray);
			for (float num = 0f; num < 1f; num += 0.01f)
			{
				vector = startPos + (endPos - startPos) * num;
				ray = new Ray(vector + new Vector3(0f, 2f, 0f), new Vector3(0f, -1f, 0f));
				if ((bool)KickStarter.settingsManager && Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer))
				{
					if (hitInfo.collider.gameObject != KickStarter.sceneSettings.navMesh.gameObject && !ignoreOthers)
					{
						return false;
					}
					continue;
				}
				return false;
			}
			return true;
		}

		protected Vector3 GetLineBreak(Vector3 startPos, Vector3 endPos)
		{
			if (startPos.y > endPos.y)
			{
				endPos.y = startPos.y;
			}
			else
			{
				startPos.y = endPos.y;
			}
			Vector3 vector = startPos;
			RaycastHit hitInfo = default(RaycastHit);
			Ray ray = default(Ray);
			for (float num = 0f; num < 1f; num += 0.01f)
			{
				vector = startPos + (endPos - startPos) * num;
				ray = new Ray(vector + new Vector3(0f, 2f, 0f), new Vector3(0f, -1f, 0f));
				if ((bool)KickStarter.settingsManager && Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer) && hitInfo.collider.gameObject != KickStarter.sceneSettings.navMesh.gameObject)
				{
					return vector;
				}
			}
			return Vector3.zero;
		}

		protected Vector3[] CreateVertexArray(Vector3 targetPos)
		{
			Mesh sharedMesh = KickStarter.sceneSettings.navMesh.transform.GetComponent<MeshCollider>().sharedMesh;
			if (sharedMesh == null)
			{
				ACDebug.LogWarning("Active NavMesh has no mesh!", KickStarter.sceneSettings.navMesh.gameObject);
				return null;
			}
			Vector3[] vertices = sharedMesh.vertices;
			List<NavMeshData> list = new List<NavMeshData>();
			Vector3[] array = vertices;
			foreach (Vector3 vertex in array)
			{
				list.Add(new NavMeshData(vertex, targetPos, KickStarter.sceneSettings.navMesh.transform));
			}
			list.Sort((NavMeshData a, NavMeshData b) => a.distance.CompareTo(b.distance));
			List<Vector3> list2 = new List<Vector3>();
			foreach (NavMeshData item in list)
			{
				list2.Add(item.vertex);
			}
			return list2.ToArray();
		}

		protected List<Vector3> FindComplexPath(Vector3 originPos, Vector3 targetPos, bool ignoreOthers)
		{
			targetPos = GetNearestToMesh(targetPos);
			pathFailed = false;
			List<Vector3> list = new List<Vector3>();
			list.Add(originPos);
			bool flag = false;
			Vector3[] array = CreateVertexArray(targetPos);
			int num = 0;
			float num2 = 0f;
			bool flag2 = false;
			bool flag3 = false;
			Vector3 item = Vector3.zero;
			List<Vector3> list2 = new List<Vector3>();
			while (!flag)
			{
				num2 = 0f;
				flag2 = false;
				flag3 = false;
				Vector3[] array2 = array;
				foreach (Vector3 vector in array2)
				{
					if (IsVertexImperfect(vector, list2.ToArray()) || !IsLineClear(vector, list[list.Count - 1], ignoreOthers))
					{
						continue;
					}
					if (IsLineClear(targetPos, vector, ignoreOthers))
					{
						if (!flag3)
						{
							float pathLength = GetPathLength(list, vector, targetPos);
							if (pathLength < num2 || !flag2)
							{
								flag2 = true;
								flag3 = true;
								item = vector;
								num2 = pathLength;
							}
						}
						else
						{
							float pathLength2 = GetPathLength(list, vector, targetPos);
							if (pathLength2 < num2)
							{
								item = vector;
								num2 = pathLength2;
							}
						}
					}
					else
					{
						if (flag3)
						{
							continue;
						}
						if (!flag2)
						{
							item = vector;
							flag2 = true;
							num2 = GetPathLength(list, vector, targetPos);
							continue;
						}
						float pathLength3 = GetPathLength(list, vector, targetPos);
						if (pathLength3 < num2)
						{
							item = vector;
							num2 = pathLength3;
						}
					}
				}
				if (flag2)
				{
					list.Add(item);
					if (flag3)
					{
						flag = true;
					}
					else
					{
						list2.Add(item);
					}
				}
				num++;
				if (num > array.Length)
				{
					pathFailed = true;
					return list;
				}
			}
			return list;
		}

		protected Vector3 GetNearestToMesh(Vector3 point)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Ray ray = default(Ray);
			ray = new Ray(point + new Vector3(0f, 2f, 0f), new Vector3(0f, -1f, 0f));
			if ((bool)KickStarter.settingsManager && !Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength, 1 << KickStarter.sceneSettings.navMesh.gameObject.layer))
			{
				Vector3[] array = CreateVertexArray(point);
				return array[0];
			}
			return point;
		}
	}
}
