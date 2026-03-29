using UnityEngine;

public class Camera_Shake : MonoBehaviour
{
	public GameObject CameraShake;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			ShakeTheCamera();
		}
	}

	private void ShakeTheCamera()
	{
		CameraShake.GetComponent<Animation>().Play();
	}
}
