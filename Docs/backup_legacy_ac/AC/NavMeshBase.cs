using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_nav_mesh_base.html")]
	public class NavMeshBase : MonoBehaviour
	{
		public bool disableRenderer = true;

		private Collider _collider;

		private MeshRenderer _meshRenderer;

		private MeshCollider _meshCollider;

		private MeshFilter _meshFilter;

		public bool ignoreCollisions = true;

		public Collider Collider
		{
			get
			{
				return _collider;
			}
		}

		private void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		private void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public void Hide()
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = false;
			}
		}

		public void Show()
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = true;
				if (_meshFilter != null && _meshCollider != null && (bool)_meshCollider.sharedMesh)
				{
					_meshFilter.mesh = _meshCollider.sharedMesh;
				}
			}
		}

		public void IgnoreNavMeshCollisions(Collider[] allColliders = null)
		{
			if (!ignoreCollisions)
			{
				return;
			}
			if (allColliders == null)
			{
				allColliders = Object.FindObjectsOfType(typeof(Collider)) as Collider[];
			}
			if (!(_collider != null) || !_collider.enabled || !_collider.gameObject.activeInHierarchy)
			{
				return;
			}
			Collider[] array = allColliders;
			foreach (Collider collider in array)
			{
				if (_collider != collider && !_collider.isTrigger && !collider.isTrigger && collider.enabled && collider.gameObject.activeInHierarchy && !(_collider is TerrainCollider))
				{
					Physics.IgnoreCollision(_collider, collider);
				}
			}
		}

		protected void BaseAwake()
		{
			_collider = GetComponent<Collider>();
			_meshRenderer = GetComponent<MeshRenderer>();
			_meshCollider = GetComponent<MeshCollider>();
			_meshFilter = GetComponent<MeshFilter>();
			if (disableRenderer)
			{
				Hide();
			}
		}
	}
}
