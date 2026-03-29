using UnityEngine;

public class RFX1_Target : MonoBehaviour
{
	public GameObject Target;

	private GameObject currentTarget;

	private RFX1_TransformMotion transformMotion;

	private void Start()
	{
		transformMotion = GetComponentInChildren<RFX1_TransformMotion>();
		UpdateTarget();
	}

	private void Update()
	{
		UpdateTarget();
	}

	private void UpdateTarget()
	{
		if (!(Target == null))
		{
			if (transformMotion == null)
			{
				Debug.Log("You must attach the target script on projectile effect!");
			}
			else if (Target != currentTarget)
			{
				currentTarget = Target;
				transformMotion.Target = currentTarget;
			}
		}
	}
}
