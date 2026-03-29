using UnityEngine;

public class demo : MonoBehaviour
{
	private GameObject[] effectObjects;

	private int effectIndex;

	private void Start()
	{
		effectObjects = Resources.LoadAll<GameObject>(string.Empty);
		for (int i = 0; i < effectObjects.Length; i++)
		{
			effectObjects[i] = Object.Instantiate(effectObjects[i]);
			effectObjects[i].transform.position = new Vector3(0f, 0f, 0f);
			effectObjects[i].SetActive(false);
		}
		effectIndex = 0;
		showCurrent();
		playCurrent();
	}

	private void playCurrent()
	{
		if (getSystem(effectIndex).isPlaying)
		{
			getSystem(effectIndex).Stop();
		}
		getSystem(effectIndex).Play();
	}

	private void stopCurrent()
	{
		getSystem(effectIndex).Stop();
	}

	private void showCurrent()
	{
		effectObjects[effectIndex].SetActive(true);
	}

	private void hideCurrent()
	{
		effectObjects[effectIndex].SetActive(false);
	}

	private void nextEffect()
	{
		hideCurrent();
		effectIndex++;
		if (effectIndex == effectObjects.Length)
		{
			effectIndex = 0;
		}
		showCurrent();
		playCurrent();
	}

	private void previousEffect()
	{
		hideCurrent();
		effectIndex--;
		if (effectIndex < 0)
		{
			effectIndex = effectObjects.Length - 1;
		}
		showCurrent();
		playCurrent();
	}

	private ParticleSystem getSystem(int i)
	{
		return effectObjects[i].transform.GetChild(0).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		playerInput();
	}

	private void playerInput()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			nextEffect();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			previousEffect();
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			Camera.main.transform.Translate(5f * Time.deltaTime * Camera.main.transform.forward);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			Camera.main.transform.Translate(-5f * Time.deltaTime * Camera.main.transform.forward);
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			playCurrent();
		}
	}
}
