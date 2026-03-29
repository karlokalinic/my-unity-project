using UnityEngine;

public class effectSpawnVolume : MonoBehaviour
{
	public Transform effectToSpawn;

	public Vector3 spawnPosition = Vector3.zero;

	public Vector3 spawnRotation = Vector3.zero;

	public bool armed;

	private void OnTriggerEnter(Collider other)
	{
		armed = true;
	}

	private void OnTriggerExit(Collider other)
	{
		armed = false;
	}

	private void Update()
	{
		if (armed && Input.GetKeyDown(KeyCode.Space))
		{
			Object.Instantiate(effectToSpawn, base.transform.position + spawnPosition, Quaternion.Euler(spawnRotation));
		}
	}
}
