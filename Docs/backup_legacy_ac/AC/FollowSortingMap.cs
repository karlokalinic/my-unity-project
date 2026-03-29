using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AC
{
	[ExecuteInEditMode]
	[AddComponentMenu("Adventure Creator/Characters/Follow SortingMap")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_follow_sorting_map.html")]
	public class FollowSortingMap : MonoBehaviour
	{
		protected enum DepthAxis
		{
			Y = 0,
			Z = 1
		}

		public bool lockSorting;

		public bool affectChildren = true;

		public bool followSortingMap = true;

		public SortingMap customSortingMap;

		public bool offsetOriginal;

		public bool livePreview;

		protected Vector3 originalDepth = Vector3.zero;

		protected DepthAxis depthAxis;

		protected Renderer[] renderers;

		protected Renderer _renderer;

		protected SortingGroup sortingGroup;

		protected List<int> offsets = new List<int>();

		protected int sortingOrder;

		protected string sortingLayer = string.Empty;

		protected SortingMap sortingMap;

		protected int sharedDepth;

		protected bool depthSet;

		public int SortingOrder
		{
			get
			{
				return sortingOrder;
			}
		}

		public string SortingLayer
		{
			get
			{
				return sortingLayer;
			}
		}

		public int SharedDepth
		{
			get
			{
				return sharedDepth;
			}
		}

		protected void Awake()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene())
			{
				return;
			}
			sortingGroup = GetComponentInChildren<SortingGroup>();
			if (sortingGroup == null)
			{
				renderers = GetComponentsInChildren<Renderer>(true);
				_renderer = GetComponent<Renderer>();
				if (_renderer == null && !affectChildren)
				{
					ACDebug.LogWarning("FollowSortingMap on " + base.gameObject.name + " must be attached alongside a Renderer component.");
				}
			}
			if (GetComponent<Char>() != null && Application.isPlaying)
			{
				ACDebug.LogWarning("The 'Follow Sorting Map' component attached to the character '" + base.gameObject.name + " is on the character's root - it should instead be placed on their sprite child.  To prevent movement locking, the Follow Sorting Map has been disabled.", this);
				base.enabled = false;
			}
			SetOriginalDepth();
		}

		protected void OnEnable()
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

		protected void Start()
		{
			if (!KickStarter.settingsManager.IsInLoadingScene())
			{
				if ((bool)KickStarter.stateHandler)
				{
					KickStarter.stateHandler.Register(this);
				}
				SetOriginalOffsets();
				if ((!followSortingMap || !(KickStarter.sceneSettings != null) || !(KickStarter.sceneSettings.sortingMap != null)) && (followSortingMap || !(customSortingMap != null)) && (livePreview || Application.isPlaying))
				{
					ACDebug.Log(base.gameObject.name + " cannot find Sorting Map to follow!");
				}
			}
		}

		protected void LateUpdate()
		{
			UpdateRenderers();
		}

		public void AfterLoad()
		{
			if (!KickStarter.settingsManager.IsInLoadingScene())
			{
				UpdateSortingMap();
				SetOriginalOffsets();
			}
		}

		public void SetDepth(int depth)
		{
			sharedDepth = depth;
			float num = (float)depth * KickStarter.sceneSettings.sharedLayerSeparationDistance;
			if (depthAxis == DepthAxis.Y)
			{
				if ((bool)base.transform.parent)
				{
					base.transform.position = base.transform.parent.position + originalDepth + Vector3.down * num;
				}
				else
				{
					base.transform.position = originalDepth + Vector3.down * num;
				}
			}
			else if ((bool)base.transform.parent)
			{
				base.transform.position = base.transform.parent.position + originalDepth + Vector3.forward * num;
			}
			else
			{
				base.transform.position = originalDepth + Vector3.forward * num;
			}
		}

		public void UpdateSortingMap()
		{
			if (followSortingMap)
			{
				if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.sortingMap != null && KickStarter.sceneSettings.sortingMap != sortingMap)
				{
					sortingMap = KickStarter.sceneSettings.sortingMap;
					SetOriginalDepth();
				}
			}
			else if (customSortingMap != null && sortingMap != customSortingMap)
			{
				sortingMap = customSortingMap;
				SetOriginalDepth();
			}
		}

		public SortingMap GetSortingMap()
		{
			if (!followSortingMap && customSortingMap != null)
			{
				return customSortingMap;
			}
			return sortingMap;
		}

		public void SetSortingMap(SortingMap _sortingMap)
		{
			if (_sortingMap == null)
			{
				followSortingMap = false;
				customSortingMap = null;
			}
			else if (KickStarter.sceneSettings.sortingMap == _sortingMap)
			{
				followSortingMap = true;
			}
			else
			{
				followSortingMap = false;
				customSortingMap = _sortingMap;
			}
			UpdateSortingMap();
		}

		public void LockSortingOrder(int order)
		{
			if (_renderer == null && sortingGroup == null)
			{
				return;
			}
			lockSorting = true;
			if (sortingGroup != null)
			{
				sortingGroup.sortingOrder = order;
				return;
			}
			if (!affectChildren)
			{
				_renderer.sortingOrder = order;
				return;
			}
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				renderer.sortingOrder = order;
			}
		}

		public void LockSortingLayer(string layer)
		{
			if (_renderer == null && sortingGroup == null)
			{
				return;
			}
			lockSorting = true;
			if (sortingGroup != null)
			{
				sortingGroup.sortingLayerName = layer;
			}
			if (!affectChildren)
			{
				_renderer.sortingLayerName = layer;
				return;
			}
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				renderer.sortingLayerName = layer;
			}
		}

		public float GetLocalScale()
		{
			if (sortingMap != null && sortingMap.affectScale)
			{
				return sortingMap.GetScale(base.transform.position) / 100f;
			}
			return 0f;
		}

		public float GetLocalSpeed()
		{
			if (sortingMap != null && sortingMap.affectScale && sortingMap.affectSpeed)
			{
				return sortingMap.GetScale(base.transform.position) / 100f;
			}
			return 1f;
		}

		protected void SetOriginalOffsets()
		{
			if (offsets.Count > 0 || sortingGroup != null)
			{
				return;
			}
			offsets = new List<int>();
			if (!offsetOriginal)
			{
				return;
			}
			if (affectChildren)
			{
				Renderer[] array = renderers;
				foreach (Renderer renderer in array)
				{
					offsets.Add(renderer.sortingOrder);
				}
			}
			else if (_renderer != null)
			{
				offsets.Add(_renderer.sortingOrder);
			}
		}

		protected void SetOriginalDepth()
		{
			if (!depthSet)
			{
				if (SceneSettings.IsTopDown())
				{
					depthAxis = DepthAxis.Y;
				}
				else
				{
					depthAxis = DepthAxis.Z;
				}
				if ((bool)base.transform.parent)
				{
					originalDepth = base.transform.position - base.transform.parent.position;
				}
				else
				{
					originalDepth = base.transform.position;
				}
				depthSet = true;
			}
		}

		protected void UpdateRenderers()
		{
			if (lockSorting || sortingMap == null)
			{
				return;
			}
			if (sortingGroup == null)
			{
				if (affectChildren)
				{
					if (renderers == null || renderers.Length == 0)
					{
						return;
					}
				}
				else if (_renderer == null)
				{
					return;
				}
			}
			if (sortingMap.sortingAreas.Count > 0)
			{
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					sortingOrder = sortingMap.sortingAreas[sortingMap.sortingAreas.Count - 1].order;
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					sortingLayer = sortingMap.sortingAreas[sortingMap.sortingAreas.Count - 1].layer;
				}
				for (int i = 0; i < sortingMap.sortingAreas.Count; i++)
				{
					if (Vector3.Angle(sortingMap.transform.forward, sortingMap.GetAreaPosition(i) - base.transform.position) < 90f)
					{
						if (sortingMap.mapType == SortingMapType.OrderInLayer)
						{
							sortingOrder = sortingMap.sortingAreas[i].order;
						}
						else if (sortingMap.mapType == SortingMapType.SortingLayer)
						{
							sortingLayer = sortingMap.sortingAreas[i].layer;
						}
						break;
					}
				}
			}
			if (sortingGroup != null)
			{
				switch (sortingMap.mapType)
				{
				case SortingMapType.OrderInLayer:
					sortingGroup.sortingOrder = sortingOrder;
					break;
				case SortingMapType.SortingLayer:
					sortingGroup.sortingLayerName = sortingLayer;
					break;
				}
				return;
			}
			if (!affectChildren)
			{
				switch (sortingMap.mapType)
				{
				case SortingMapType.OrderInLayer:
					_renderer.sortingOrder = sortingOrder;
					if (offsetOriginal && offsets.Count > 0)
					{
						_renderer.sortingOrder += offsets[0];
					}
					break;
				case SortingMapType.SortingLayer:
					_renderer.sortingLayerName = sortingLayer;
					if (offsetOriginal && offsets.Count > 0)
					{
						_renderer.sortingOrder = offsets[0];
					}
					else
					{
						_renderer.sortingOrder = 0;
					}
					break;
				}
				return;
			}
			for (int j = 0; j < renderers.Length; j++)
			{
				switch (sortingMap.mapType)
				{
				case SortingMapType.OrderInLayer:
					renderers[j].sortingOrder = sortingOrder;
					if (offsetOriginal && offsets.Count > j)
					{
						renderers[j].sortingOrder += offsets[j];
					}
					break;
				case SortingMapType.SortingLayer:
					renderers[j].sortingLayerName = sortingLayer;
					if (offsetOriginal && offsets.Count > j)
					{
						renderers[j].sortingOrder = offsets[j];
					}
					else
					{
						renderers[j].sortingOrder = 0;
					}
					break;
				}
			}
		}
	}
}
