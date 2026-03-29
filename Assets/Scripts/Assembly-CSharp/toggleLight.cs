using UnityEngine;

public class toggleLight : MonoBehaviour
{
	public GameObject playerLight;

	public bool lightOn;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return) && !lightOn)
		{
			lightOn = true;
			playerLight.SetActive(true);
		}
		else if (lightOn && Input.GetKeyDown(KeyCode.Return))
		{
			lightOn = false;
			playerLight.SetActive(false);
		}
	}
}
