using UnityEngine;

public class RFX1_LegacyQueue : MonoBehaviour
{
	public int Queue = 3001;

	private void Awake()
	{
		GetComponent<Renderer>().material.renderQueue = Queue;
	}

	private void Update()
	{
	}
}
