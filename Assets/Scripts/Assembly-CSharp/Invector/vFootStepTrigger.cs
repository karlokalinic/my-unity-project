using UnityEngine;

namespace Invector
{
	public class vFootStepTrigger : MonoBehaviour
	{
		protected Collider _trigger;

		protected vFootStep _fT;

		protected Collider lastCollider;

		protected FootStepObject footstepObj;

		public Collider trigger
		{
			get
			{
				if (_trigger == null)
				{
					_trigger = GetComponent<Collider>();
				}
				return _trigger;
			}
		}

		private void OnDrawGizmos()
		{
			if ((bool)trigger)
			{
				Color green = Color.green;
				green.a = 0.5f;
				Gizmos.color = green;
				if (trigger is SphereCollider)
				{
					Gizmos.DrawSphere(trigger.bounds.center, (trigger as SphereCollider).radius);
				}
			}
		}

		private void Start()
		{
			_fT = GetComponentInParent<vFootStep>();
			if (_fT == null)
			{
				Debug.Log(base.gameObject.name + " can't find the FootStepFromTexture");
				base.gameObject.SetActive(false);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!(_fT == null))
			{
				if (lastCollider == null || lastCollider != other || footstepObj == null)
				{
					footstepObj = new FootStepObject(base.transform, other);
					lastCollider = other;
				}
				if (footstepObj.isTerrain)
				{
					_fT.StepOnTerrain(footstepObj);
				}
				else
				{
					_fT.StepOnMesh(footstepObj);
				}
			}
		}
	}
}
