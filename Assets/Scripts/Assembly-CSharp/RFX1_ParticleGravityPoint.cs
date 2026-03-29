using UnityEngine;

[ExecuteInEditMode]
public class RFX1_ParticleGravityPoint : MonoBehaviour
{
	public Transform target;

	public float Force = 1f;

	public AnimationCurve ForceByTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

	public float ForceLifeTime = 1f;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private ParticleSystem.MainModule mainModule;

	private float startTime;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		mainModule = ps.main;
	}

	private void OnEnable()
	{
		startTime = Time.time;
	}

	private void LateUpdate()
	{
		int maxParticles = mainModule.maxParticles;
		if (particles == null || particles.Length < maxParticles)
		{
			particles = new ParticleSystem.Particle[maxParticles];
		}
		int num = ps.GetParticles(particles);
		float num2 = ForceByTime.Evaluate((Time.time - startTime) / ForceLifeTime) * Time.deltaTime * Force;
		Vector3 vector = Vector3.zero;
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = base.transform.InverseTransformPoint(target.position);
		}
		if (mainModule.simulationSpace == ParticleSystemSimulationSpace.World)
		{
			vector = target.position;
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = Vector3.Normalize(vector - particles[i].position);
			Vector3 vector3 = vector2 * num2;
			particles[i].velocity += vector3;
		}
		ps.SetParticles(particles, num);
	}
}
