using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/3rd-party/World-space cursor example")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_world_space_cursor_example.html")]
	public class WorldSpaceCursorExample : MonoBehaviour
	{
		public LayerMask collisionLayer;

		public float minDistance = 1f;

		public float maxDistance = 30f;

		private RaycastHit hit;

		private Ray ray;

		private Collider ownCollider;

		private void Start()
		{
			ownCollider = GetComponent<Collider>();
			KickStarter.playerInput.InputMousePositionDelegate = CustomMousePosition;
		}

		private Vector2 CustomMousePosition(bool cursorIsLocked)
		{
			if (cursorIsLocked)
			{
				ray = KickStarter.CameraMain.ViewportPointToRay(new Vector2(0.5f, 0.5f));
			}
			else
			{
				ray = KickStarter.CameraMain.ScreenPointToRay(Input.mousePosition);
			}
			if (Physics.Raycast(ray, out hit, maxDistance, collisionLayer))
			{
				if (ownCollider == null || hit.collider != ownCollider)
				{
					SetPosition(hit.point);
				}
				else
				{
					SetPosition(base.transform.position);
				}
			}
			else
			{
				SetPosition(base.transform.position);
			}
			return KickStarter.CameraMain.WorldToScreenPoint(base.transform.position);
		}

		private void SetPosition(Vector3 targetPosition)
		{
			float magnitude = (targetPosition - KickStarter.CameraMain.transform.position).magnitude;
			magnitude = Mathf.Clamp(magnitude, minDistance, maxDistance);
			base.transform.position = KickStarter.CameraMain.transform.position + ray.direction.normalized * magnitude;
		}
	}
}
