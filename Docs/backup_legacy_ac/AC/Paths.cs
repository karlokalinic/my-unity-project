using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_paths.html")]
	public class Paths : MonoBehaviour
	{
		public List<Vector3> nodes = new List<Vector3>();

		public List<NodeCommand> nodeCommands = new List<NodeCommand>();

		public ActionListSource commandSource;

		public AC_PathType pathType = AC_PathType.ForwardOnly;

		public PathSpeed pathSpeed;

		public bool teleportToStart;

		public bool affectY;

		public float nodePause;

		public Vector3 Destination
		{
			get
			{
				return nodes[nodes.Count - 1];
			}
		}

		protected void Awake()
		{
			if (nodePause < 0f)
			{
				nodePause = 0f;
			}
			if (nodes == null || nodes.Count == 0)
			{
				nodes.Add(base.transform.position);
			}
			else
			{
				nodes[0] = base.transform.position;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = ACEditorPrefs.PathGizmoColor;
			int count = nodes.Count;
			if (nodes.Count > 0)
			{
				nodes[0] = base.transform.position;
			}
			if (pathType == AC_PathType.IsRandom && count > 1)
			{
				for (int i = 1; i < count; i++)
				{
					for (int j = 0; j < count; j++)
					{
						if (i != j)
						{
							ConnectNodes(i, j);
						}
					}
				}
				return;
			}
			if (count > 1)
			{
				for (int i = 1; i < count; i++)
				{
					Gizmos.DrawIcon(nodes[i], string.Empty, true);
					ConnectNodes(i, i - 1);
				}
			}
			if (pathType == AC_PathType.Loop && !teleportToStart && count > 2)
			{
				ConnectNodes(count - 1, 0);
			}
		}

		public bool WillStopAtNextNode(int currentNode)
		{
			if (GetNextNode(currentNode, currentNode - 1, false) == -1)
			{
				return true;
			}
			return false;
		}

		public void RecalculateToCenter(Vector3 startPosition, float maxNodeDistance = -1f)
		{
			Vector3 vector = startPosition;
			if (SceneSettings.ActInScreenSpace())
			{
				vector = AdvGame.GetScreenNavMesh(vector);
			}
			Vector3[] pointArray;
			if (KickStarter.navigationManager != null)
			{
				pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(base.transform.position, vector);
			}
			else
			{
				List<Vector3> list = new List<Vector3>();
				list.Add(vector);
				pointArray = list.ToArray();
			}
			pointArray = SetMaxDistances(pointArray, maxNodeDistance);
			BuildNavPath(pointArray);
			pathType = AC_PathType.ReverseOnly;
		}

		public void BuildNavPath(Vector3[] pointData)
		{
			if (pointData == null || pointData.Length <= 0)
			{
				return;
			}
			pathType = AC_PathType.ForwardOnly;
			affectY = false;
			nodePause = 0f;
			List<Vector3> list = new List<Vector3>();
			list.Clear();
			list.Add(base.transform.position);
			nodeCommands.Clear();
			for (int i = 0; i < pointData.Length; i++)
			{
				if (i == 0)
				{
					if (SceneSettings.IsUnity2D())
					{
						Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.y);
						Vector2 vector2 = new Vector2(pointData[0].x, pointData[0].y);
						if ((vector - vector2).magnitude < 0.001f)
						{
							continue;
						}
					}
					else
					{
						Vector3 vector3 = new Vector3(base.transform.position.x, pointData[0].y, base.transform.position.z);
						if ((vector3 - pointData[0]).magnitude < 0.001f)
						{
							continue;
						}
					}
				}
				list.Add(pointData[i]);
			}
			nodes = list;
		}

		public int GetNextNode(int currentNode, int prevNode, bool playerControlled)
		{
			int count = nodes.Count;
			if (count == 1)
			{
				return -1;
			}
			if (playerControlled)
			{
				if (currentNode == 0)
				{
					return 1;
				}
				if (currentNode >= count - 1)
				{
					return -1;
				}
				return currentNode + 1;
			}
			if (pathType == AC_PathType.ForwardOnly)
			{
				if (currentNode == count - 1)
				{
					return -1;
				}
				return currentNode + 1;
			}
			if (pathType == AC_PathType.Loop)
			{
				if (currentNode == count - 1)
				{
					return 0;
				}
				return currentNode + 1;
			}
			if (pathType == AC_PathType.ReverseOnly)
			{
				if (currentNode == 0)
				{
					return -1;
				}
				return currentNode - 1;
			}
			if (pathType == AC_PathType.PingPong)
			{
				if (prevNode > currentNode)
				{
					if (currentNode == 0)
					{
						return 1;
					}
					return currentNode - 1;
				}
				if (currentNode == count - 1)
				{
					return currentNode - 1;
				}
				return currentNode + 1;
			}
			if (pathType == AC_PathType.IsRandom)
			{
				if (count > 0)
				{
					int num;
					for (num = Random.Range(0, count); num == currentNode; num = Random.Range(0, count))
					{
					}
					return num;
				}
				return 0;
			}
			return -1;
		}

		public float GetLengthToNode(int n)
		{
			if (n > 0 && nodes.Count > n)
			{
				float num = 0f;
				for (int i = 1; i <= n; i++)
				{
					num += Vector3.Distance(nodes[i - 1], nodes[i]);
				}
				return num;
			}
			return 0f;
		}

		public float GetLengthBetweenNodes(int a, int b)
		{
			if (a == b)
			{
				return 0f;
			}
			if (b < a)
			{
				int num = a;
				a = b;
				b = num;
			}
			float num2 = 0f;
			for (int i = a + 1; i <= b; i++)
			{
				num2 += Vector3.Distance(nodes[i - 1], nodes[i]);
			}
			return num2;
		}

		public float GetTotalLength()
		{
			if (nodes.Count > 1)
			{
				return GetLengthToNode(nodes.Count - 1);
			}
			return 0f;
		}

		protected void ConnectNodes(int a, int b)
		{
			Vector3 vector = nodes[a] + Vector3.up * 0.001f;
			Vector3 to = nodes[b] + Vector3.up * 0.001f;
			Gizmos.DrawLine(vector, to);
		}

		protected Vector3[] SetMaxDistances(Vector3[] pointArray, float maxNodeDistance)
		{
			if (maxNodeDistance <= 0f || pointArray.Length <= 1)
			{
				return pointArray;
			}
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < pointArray.Length; i++)
			{
				Vector3 vector = pointArray[i];
				if (i == 0)
				{
					list.Add(vector);
					continue;
				}
				Vector3 vector2 = pointArray[i - 1];
				float num = Vector3.Distance(vector, vector2);
				float f = num / maxNodeDistance;
				int num2 = Mathf.FloorToInt(f);
				if (num2 > 0)
				{
					float num3 = num / (float)(num2 + 1);
					Vector3 normalized = (vector - vector2).normalized;
					for (int j = 1; j <= num2; j++)
					{
						Vector3 item = vector2 + j * normalized * num3;
						list.Add(item);
					}
				}
				list.Add(vector);
			}
			return list.ToArray();
		}
	}
}
