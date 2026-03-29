using UnityEngine;

public class RFX1_ShieldInteraction : MonoBehaviour
{
	private RFX1_TransformMotion transformMotion;

	private SphereCollider coll;

	private void Start()
	{
		transformMotion = GetComponentInChildren<RFX1_TransformMotion>();
		if (transformMotion != null)
		{
			transformMotion.CollisionEnter += TransformMotion_CollisionEnter;
			coll = transformMotion.gameObject.AddComponent<SphereCollider>();
			coll.radius = 0.1f;
			coll.isTrigger = true;
		}
	}

	private void TransformMotion_CollisionEnter(object sender, RFX1_TransformMotion.RFX1_CollisionInfo e)
	{
		RFX1_ShieldCollisionTrigger componentInChildren = e.Hit.transform.GetComponentInChildren<RFX1_ShieldCollisionTrigger>();
		if (!(componentInChildren == null))
		{
			componentInChildren.OnCollision(e.Hit, base.gameObject);
			coll.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (coll != null)
		{
			coll.enabled = true;
		}
	}

	private void Update()
	{
	}
}
