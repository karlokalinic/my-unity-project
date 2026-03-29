using UnityEngine;

public class vResetPos : MonoBehaviour
{
	public Transform startPos;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			other.gameObject.transform.position = startPos.position;
		}
	}
}
