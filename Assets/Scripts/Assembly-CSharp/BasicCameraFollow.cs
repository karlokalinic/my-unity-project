using UnityEngine;

public class BasicCameraFollow : MonoBehaviour
{
	[SerializeField]
	private GameObject player;

	private void Update()
	{
		base.transform.position = new Vector3(player.transform.position.x, 8f, player.transform.position.z - 10f);
	}
}
