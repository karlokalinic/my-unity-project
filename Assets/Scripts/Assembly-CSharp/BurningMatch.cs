using System.Collections;
using UnityEngine;

public class BurningMatch : MonoBehaviour
{
	public GameObject MatchMesh;

	public GameObject MatchAnim;

	public GameObject Flame;

	public ParticleSystem SmokeParticlesA;

	public ParticleSystem SmokeParticlesB;

	public ParticleSystem SmokeParticlesC;

	public AudioSource MatchLight;

	private float offset;

	private Vector2 offsetVector;

	private Renderer matchMeshRenderer;

	private bool MatchLit;

	private bool SmokeParticlesA_On;

	private bool SmokeParticlesB_On;

	private bool SmokeParticlesC_On;

	private void Start()
	{
		Flame.SetActive(false);
		SmokeParticlesA.Stop();
		SmokeParticlesB.Stop();
		SmokeParticlesC.Stop();
		matchMeshRenderer = MatchMesh.GetComponent<Renderer>();
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire1") && !MatchLit)
		{
			MatchLight.Play();
			StartCoroutine("Fuse");
		}
	}

	private IEnumerator Fuse()
	{
		MatchLit = true;
		Flame.SetActive(true);
		MatchAnim.GetComponent<Animation>().Play();
		Flame.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(0.2f);
		offset = 0.09f;
		while (offset < 0.1f)
		{
			offset += Time.deltaTime * 0.005f;
			Vector2 offsetVector = new Vector2(offset, offset);
			matchMeshRenderer.material.SetTextureOffset("_DetailAlbedoMap", offsetVector);
			yield return 0;
		}
		yield return new WaitForSeconds(5f);
		while (offset < 0.43f)
		{
			offset += Time.deltaTime * 0.0165f;
			Vector2 offsetVector2 = new Vector2(offset, offset);
			matchMeshRenderer.material.SetTextureOffset("_DetailAlbedoMap", offsetVector2);
			if (offset > 0.22f)
			{
				if (!SmokeParticlesA_On)
				{
					SmokeParticlesA.Play();
				}
				SmokeParticlesA_On = true;
			}
			if (offset > 0.27f)
			{
				if (!SmokeParticlesB_On)
				{
					SmokeParticlesB.Play();
				}
				SmokeParticlesB_On = true;
			}
			if (offset > 0.43f)
			{
				if (!SmokeParticlesC_On)
				{
					SmokeParticlesC.Play();
				}
				SmokeParticlesC_On = true;
			}
			yield return 0;
		}
		offset = 0f;
		Flame.SetActive(false);
	}
}
