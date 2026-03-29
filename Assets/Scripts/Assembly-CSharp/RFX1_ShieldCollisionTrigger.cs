using System;
using UnityEngine;

public class RFX1_ShieldCollisionTrigger : MonoBehaviour
{
	public float DetectRange;

	public GameObject[] EffectOnCollision;

	public float DestroyTimeDelay = 5f;

	public bool CollisionEffectInWorldSpace = true;

	public float CollisionOffset;

	private const string layerName = "Collision";

	public event EventHandler<RFX1_ShieldCollisionInfo> CollisionEnter;

	public event EventHandler<RFX1_ShieldDetectInfo> Detected;

	private void Start()
	{
	}

	private void Update()
	{
		if (DetectRange < 0.001f)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, DetectRange);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.name.EndsWith("Collision"))
			{
				EventHandler<RFX1_ShieldDetectInfo> eventHandler = this.Detected;
				if (eventHandler != null)
				{
					eventHandler(this, new RFX1_ShieldDetectInfo
					{
						DetectedGameObject = collider.gameObject
					});
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(base.transform.position, DetectRange);
		}
	}

	public void OnCollision(RaycastHit hit, GameObject sender = null)
	{
		EventHandler<RFX1_ShieldCollisionInfo> eventHandler = this.CollisionEnter;
		if (eventHandler != null)
		{
			eventHandler(this, new RFX1_ShieldCollisionInfo
			{
				Hit = hit
			});
		}
		GameObject[] effectOnCollision = EffectOnCollision;
		foreach (GameObject original in effectOnCollision)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original, hit.point + hit.normal * CollisionOffset, default(Quaternion));
			gameObject.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
			if (!CollisionEffectInWorldSpace)
			{
				gameObject.transform.parent = base.transform;
			}
			UnityEngine.Object.Destroy(gameObject, DestroyTimeDelay);
		}
	}
}
