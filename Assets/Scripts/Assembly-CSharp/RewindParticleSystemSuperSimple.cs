using UnityEngine;

public class RewindParticleSystemSuperSimple : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private float[] simulationTimes;

	public float startTime = 2f;

	public float simulationSpeedScale = 1f;

	private void Initialize()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>(false);
		simulationTimes = new float[particleSystems.Length];
	}

	private void OnEnable()
	{
		if (particleSystems == null)
		{
			Initialize();
		}
		for (int i = 0; i < simulationTimes.Length; i++)
		{
			simulationTimes[i] = 0f;
		}
		particleSystems[0].Simulate(startTime, true, false, true);
	}

	private void Update()
	{
		particleSystems[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		for (int num = particleSystems.Length - 1; num >= 0; num--)
		{
			bool useAutoRandomSeed = particleSystems[num].useAutoRandomSeed;
			particleSystems[num].useAutoRandomSeed = false;
			particleSystems[num].Play(false);
			float num2 = ((!particleSystems[num].main.useUnscaledTime) ? Time.deltaTime : Time.unscaledDeltaTime);
			simulationTimes[num] -= num2 * particleSystems[num].main.simulationSpeed * simulationSpeedScale;
			float num3 = startTime + simulationTimes[num];
			particleSystems[num].Simulate(num3, false, false, true);
			particleSystems[num].useAutoRandomSeed = useAutoRandomSeed;
			if (num3 < 0f)
			{
				particleSystems[num].Play(false);
				particleSystems[num].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
	}
}
