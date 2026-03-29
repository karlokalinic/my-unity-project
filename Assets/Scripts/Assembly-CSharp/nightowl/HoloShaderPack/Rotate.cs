using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class Rotate : MonoBehaviour
	{
		public float Speed = 0.1f;

		public Vector3 Angle = Vector3.up;

		private void Update()
		{
			base.transform.Rotate(Angle, Speed);
		}
	}
}
