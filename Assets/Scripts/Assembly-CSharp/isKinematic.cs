using UnityEngine;

public class isKinematic : MonoBehaviour
{
	public Component[] rigidbodies;

	private void Start()
	{
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		Component[] array = rigidbodies;
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody rigidbody = (Rigidbody)array[i];
			rigidbody.isKinematic = true;
		}
	}

	private void Update()
	{
	}
}
