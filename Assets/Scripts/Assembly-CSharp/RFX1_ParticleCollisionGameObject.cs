using System.Collections.Generic;
using UnityEngine;

public class RFX1_ParticleCollisionGameObject : MonoBehaviour
{
	public GameObject InstancedGO;

	public float DestroyDelay = 5f;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private ParticleSystem initiatorPS;

	private void OnEnable()
	{
		collisionEvents.Clear();
		initiatorPS = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject other)
	{
		int num = initiatorPS.GetCollisionEvents(other, collisionEvents);
		for (int i = 0; i < num; i++)
		{
			Object.Destroy(Object.Instantiate(InstancedGO, collisionEvents[i].intersection, default(Quaternion)), DestroyDelay);
		}
	}
}
