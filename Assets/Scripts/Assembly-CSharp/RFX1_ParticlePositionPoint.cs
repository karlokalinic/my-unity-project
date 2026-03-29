using UnityEngine;

public class RFX1_ParticlePositionPoint : MonoBehaviour
{
	[HideInInspector]
	public Vector3 Position;

	public RFX1_ShieldCollisionTrigger ShieldCollisionTrigger;

	public float Force = 1f;

	public AnimationCurve ForceByTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

	public float ForceLifeTime = 1f;

	private bool canUpdate;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private ParticleSystem.MainModule mainModule;

	private float startTime;

	private void Start()
	{
		ShieldCollisionTrigger.CollisionEnter += ShieldCollisionTrigger_CollisionEnter;
		ShieldCollisionTrigger.Detected += ShieldCollisionTrigger_Detected;
		ps = GetComponent<ParticleSystem>();
		mainModule = ps.main;
	}

	private void ShieldCollisionTrigger_Detected(object sender, RFX1_ShieldDetectInfo e)
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(e.DetectedGameObject.transform.position, e.DetectedGameObject.transform.forward, out hitInfo, 10f))
		{
			Position = hitInfo.point;
			ManualOnEnable();
		}
	}

	private void ShieldCollisionTrigger_CollisionEnter(object sender, RFX1_ShieldCollisionInfo e)
	{
		Position = e.Hit.point;
		ManualOnEnable();
	}

	public void ManualOnEnable()
	{
		CancelInvoke("ManualOnDisable");
		startTime = Time.time;
		canUpdate = true;
		Invoke("ManualOnDisable", ForceLifeTime);
	}

	private void ManualOnDisable()
	{
		canUpdate = false;
	}

	private void LateUpdate()
	{
		if (canUpdate)
		{
			int maxParticles = mainModule.maxParticles;
			if (particles == null || particles.Length < maxParticles)
			{
				particles = new ParticleSystem.Particle[maxParticles];
			}
			ps.GetParticles(particles);
			float num = ForceByTime.Evaluate((Time.time - startTime) / ForceLifeTime) * Time.deltaTime * Force;
			Vector3 vector = Vector3.zero;
			if (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local)
			{
				vector = base.transform.InverseTransformPoint(Position);
			}
			if (mainModule.simulationSpace == ParticleSystemSimulationSpace.World)
			{
				vector = Position;
			}
			int particleCount = ps.particleCount;
			for (int i = 0; i < particleCount; i++)
			{
				Vector3 vector2 = Vector3.Normalize(vector - particles[i].position);
				Vector3 vector3 = vector2 * num;
				particles[i].position += vector3;
			}
			ps.SetParticles(particles, particleCount);
		}
	}
}
