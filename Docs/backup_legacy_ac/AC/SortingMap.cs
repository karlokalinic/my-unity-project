using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_sorting_map.html")]
	public class SortingMap : MonoBehaviour
	{
		public SortingMapType mapType = SortingMapType.OrderInLayer;

		public List<SortingArea> sortingAreas = new List<SortingArea>();

		public bool affectScale;

		public bool affectSpeed = true;

		public int originScale = 100;

		public SortingMapScaleType sortingMapScaleType;

		public AnimationCurve scalingAnimationCurve;

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		protected void OnDrawGizmos()
		{
			Vector3 vector = base.transform.right * 0.1f;
			float num = ((!affectScale || sortingMapScaleType != SortingMapScaleType.Linear) ? 1f : ((float)originScale / 100f));
			Gizmos.DrawLine(base.transform.position - vector * num, base.transform.position + vector * num);
			for (int i = 0; i < sortingAreas.Count; i++)
			{
				num = ((!affectScale || sortingMapScaleType != SortingMapScaleType.Linear) ? 1f : ((float)sortingAreas[i].scale / 100f));
				Gizmos.color = sortingAreas[i].color;
				Gizmos.DrawIcon(GetAreaPosition(i), string.Empty, true);
				Gizmos.DrawLine(GetAreaPosition(i) - vector * num, GetAreaPosition(i) + vector * num);
				Vector3 vector2 = ((i != 0) ? GetAreaPosition(i - 1) : base.transform.position);
				float num2 = ((!affectScale || sortingMapScaleType != SortingMapScaleType.Linear) ? 1f : ((i != 0) ? ((float)sortingAreas[i - 1].scale / 100f) : ((float)originScale / 100f)));
				Gizmos.DrawLine(vector2 + vector * num2, GetAreaPosition(i) + vector * num);
				Gizmos.DrawLine(vector2 - vector * num2, GetAreaPosition(i) - vector * num);
			}
		}

		public void UpdateSimilarFollowers()
		{
			if (KickStarter.sceneSettings.sharedLayerSeparationDistance <= 0f)
			{
				return;
			}
			foreach (SortingArea sortingArea in sortingAreas)
			{
				List<FollowSortingMap> list = new List<FollowSortingMap>();
				for (int i = 0; i < KickStarter.stateHandler.FollowSortingMaps.Count; i++)
				{
					if (KickStarter.stateHandler.FollowSortingMaps[i].GetSortingMap() == this && ((mapType == SortingMapType.OrderInLayer && KickStarter.stateHandler.FollowSortingMaps[i].SortingOrder == sortingArea.order) || (mapType == SortingMapType.SortingLayer && KickStarter.stateHandler.FollowSortingMaps[i].SortingLayer == sortingArea.layer)))
					{
						list.Add(KickStarter.stateHandler.FollowSortingMaps[i]);
					}
				}
				switch (list.Count)
				{
				case 1:
					list[0].SetDepth(0);
					continue;
				case 0:
					continue;
				}
				list.Sort(SortByScreenPosition);
				for (int j = 0; j < list.Count; j++)
				{
					list[j].SetDepth(j);
				}
			}
		}

		public Vector3 GetAreaPosition(int i)
		{
			return base.transform.position + base.transform.forward * sortingAreas[i].z;
		}

		public float GetScale(Vector3 followPosition)
		{
			if (!affectScale)
			{
				return 1f;
			}
			if (sortingAreas.Count == 0)
			{
				return originScale;
			}
			if (Vector3.Angle(base.transform.forward, base.transform.position - followPosition) < 90f)
			{
				if (sortingMapScaleType == SortingMapScaleType.AnimationCurve)
				{
					float a = scalingAnimationCurve.Evaluate(0f) * 100f;
					return Mathf.Max(a, 1f);
				}
				return originScale;
			}
			if (Vector3.Angle(base.transform.forward, GetAreaPosition(sortingAreas.Count - 1) - followPosition) > 90f)
			{
				if (sortingMapScaleType == SortingMapScaleType.AnimationCurve)
				{
					float a2 = scalingAnimationCurve.Evaluate(1f) * 100f;
					return Mathf.Max(a2, 1f);
				}
				return sortingAreas[sortingAreas.Count - 1].scale;
			}
			if (sortingMapScaleType == SortingMapScaleType.AnimationCurve)
			{
				int num = sortingAreas.Count - 1;
				float num2 = Vector3.Angle(base.transform.forward, GetAreaPosition(num) - followPosition);
				float time = 1f - Vector3.Distance(GetAreaPosition(num), followPosition) / sortingAreas[num].z * Mathf.Cos((float)Math.PI / 180f * num2);
				float a3 = scalingAnimationCurve.Evaluate(time) * 100f;
				return Mathf.Max(a3, 1f);
			}
			for (int i = 0; i < sortingAreas.Count; i++)
			{
				float num3 = Vector3.Angle(base.transform.forward, GetAreaPosition(i) - followPosition);
				if (num3 < 90f)
				{
					float num4 = 0f;
					if (i > 0)
					{
						num4 = sortingAreas[i - 1].z;
					}
					float num5 = 1f - Vector3.Distance(GetAreaPosition(i), followPosition) / (sortingAreas[i].z - num4) * Mathf.Cos((float)Math.PI / 180f * num3);
					float num6 = originScale;
					if (i > 0)
					{
						num6 = sortingAreas[i - 1].scale;
					}
					return num6 + num5 * ((float)sortingAreas[i].scale - num6);
				}
			}
			return 1f;
		}

		public void SetInBetweenScales()
		{
			if (sortingAreas.Count >= 2)
			{
				float num = sortingAreas[sortingAreas.Count - 1].scale;
				float z = sortingAreas[sortingAreas.Count - 1].z;
				for (int i = 0; i < sortingAreas.Count - 1; i++)
				{
					float num2 = sortingAreas[i].z / z * (num - (float)originScale) + (float)originScale;
					sortingAreas[i].scale = (int)num2;
				}
			}
		}

		protected static int SortByScreenPosition(FollowSortingMap o1, FollowSortingMap o2)
		{
			return KickStarter.CameraMain.WorldToScreenPoint(o1.transform.position).y.CompareTo(KickStarter.CameraMain.WorldToScreenPoint(o2.transform.position).y);
		}
	}
}
