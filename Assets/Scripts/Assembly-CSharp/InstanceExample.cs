using UnityEngine;

public class InstanceExample : MonoBehaviour
{
	public static InstanceExample Instance;

	public ParticleSystem effectA;

	public ParticleSystem effectB;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("Multiple instances of InstanceExample script!");
		}
		Instance = this;
	}

	private void Update()
	{
		Instance.Explosion(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
	}

	public void Explosion(Vector3 position)
	{
		instantiate(effectA, position);
		instantiate(effectB, position);
	}

	private ParticleSystem instantiate(ParticleSystem prefab, Vector3 position)
	{
		ParticleSystem particleSystem = Object.Instantiate(prefab, position, Quaternion.identity);
		Object.Destroy(particleSystem.gameObject, particleSystem.startLifetime);
		return particleSystem;
	}
}
