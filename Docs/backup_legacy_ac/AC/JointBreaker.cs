using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_joint_breaker.html")]
	public class JointBreaker : MonoBehaviour
	{
		protected FixedJoint fixedJoint;

		protected void Awake()
		{
			fixedJoint = GetComponent<FixedJoint>();
		}

		protected void OnJointBreak(float breakForce)
		{
			fixedJoint.connectedBody.GetComponent<Moveable_PickUp>().UnsetFixedJoint();
			Object.Destroy(base.gameObject);
		}
	}
}
