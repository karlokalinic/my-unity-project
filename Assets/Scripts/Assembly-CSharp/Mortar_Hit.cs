using System.Collections;
using UnityEngine;

public class Mortar_Hit : MonoBehaviour
{
	public GameObject DustParticles;

	public GameObject LightBulbAnim;

	public GameObject CameraShake;

	public AudioSource ExplosionAudio;

	public Light BulbLight;

	public GameObject LightBulb;

	public GameObject LightBulbOff;

	private float BulbIntensity = 3.5f;

	private float RocketHitting;

	private void Start()
	{
		DustParticles.SetActive(false);
		LightBulbOff.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			StartCoroutine("RocketStrike");
		}
		BulbLight.intensity = BulbIntensity;
		if (RocketHitting == 0f)
		{
			BulbIntensity = Random.Range(3.4f, 3.65f);
		}
		if (RocketHitting == 1f)
		{
			BulbIntensity = Random.Range(2f, 4f);
		}
		if (RocketHitting == 2f)
		{
			BulbIntensity = Random.Range(3f, 4f);
		}
	}

	private IEnumerator RocketStrike()
	{
		ExplosionAudio.Play();
		yield return new WaitForSeconds(1.5f);
		DustParticles.SetActive(false);
		DustParticles.SetActive(true);
		RocketHitting = 1f;
		DustParticles.SetActive(false);
		DustParticles.SetActive(true);
		LightBulbAnim.GetComponent<Animation>().Play();
		CameraShake.GetComponent<Animation>().Play();
		LightBulb.SetActive(false);
		LightBulbOff.SetActive(true);
		yield return new WaitForSeconds(0.4f);
		LightBulb.SetActive(true);
		LightBulbOff.SetActive(false);
		yield return new WaitForSeconds(0.4f);
		LightBulb.SetActive(false);
		LightBulbOff.SetActive(true);
		yield return new WaitForSeconds(0.2f);
		RocketHitting = 2f;
		LightBulb.SetActive(true);
		LightBulbOff.SetActive(false);
		yield return new WaitForSeconds(0.2f);
		LightBulb.SetActive(false);
		LightBulbOff.SetActive(true);
		yield return new WaitForSeconds(0.1f);
		LightBulb.SetActive(true);
		LightBulbOff.SetActive(false);
		yield return new WaitForSeconds(1f);
		RocketHitting = 0f;
	}

	private void FlickerBulb()
	{
	}
}
