using System.Collections;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_click_marker.html")]
	public class ClickMarker : MonoBehaviour
	{
		public float lifeTime = 0.5f;

		protected float startTime;

		protected Vector3 startScale;

		protected Vector3 endScale = Vector3.zero;

		protected void Start()
		{
			Object.Destroy(base.gameObject, lifeTime);
			if (lifeTime > 0f)
			{
				startTime = Time.time;
				startScale = base.transform.localScale;
			}
			StartCoroutine("ShrinkMarker");
		}

		protected IEnumerator ShrinkMarker()
		{
			while (lifeTime > 0f)
			{
				base.transform.localScale = Vector3.Lerp(startScale, endScale, AdvGame.Interpolate(startTime, lifeTime, MoveMethod.EaseIn));
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
