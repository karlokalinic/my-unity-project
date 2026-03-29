using UnityEngine;

public class RFX1_DeactivateByTime : MonoBehaviour
{
	public float DeactivateTime = 3f;

	private void OnEnable()
	{
		Invoke("DeactivateThis", DeactivateTime);
	}

	private void OnDisable()
	{
		CancelInvoke("DeactivateThis");
	}

	private void DeactivateThis()
	{
		base.gameObject.SetActive(false);
	}
}
