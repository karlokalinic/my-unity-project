using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class Scaler : MonoBehaviour
	{
		public float MinScale;

		public float MaxScale = 1f;

		public float Speed = 1f;

		private void Update()
		{
			float y = MinScale + (MaxScale - MinScale) * (Mathf.Sin(Time.time * Speed) * 0.5f + 0.5f);
			base.transform.localScale = new Vector3(base.transform.localScale.x, y, base.transform.localScale.z);
		}
	}
}
