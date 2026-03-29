using UnityEngine;

public class RewindParticleSystem : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private float[] startTimes;

	private float[] simulationTimes;

	public float startTime = 2f;

	public float simulationSpeedScale = 1f;

	public bool useFixedDeltaTime = true;

	private bool gameObjectDeactivated;

	private void OnEnable()
	{
		bool flag = particleSystems == null;
		if (flag)
		{
			particleSystems = GetComponentsInChildren<ParticleSystem>(false);
			startTimes = new float[particleSystems.Length];
			simulationTimes = new float[particleSystems.Length];
		}
		for (int num = particleSystems.Length - 1; num >= 0; num--)
		{
			simulationTimes[num] = 0f;
			if (flag || gameObjectDeactivated)
			{
				startTimes[num] = startTime;
				particleSystems[num].Simulate(startTimes[num], false, false, useFixedDeltaTime);
			}
			else
			{
				startTimes[num] = particleSystems[num].time;
			}
		}
	}

	private void OnDisable()
	{
		particleSystems[0].Play(true);
		gameObjectDeactivated = !base.gameObject.activeInHierarchy;
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
			float num3 = startTimes[num] + simulationTimes[num];
			particleSystems[num].Simulate(num3, false, false, useFixedDeltaTime);
			particleSystems[num].useAutoRandomSeed = useAutoRandomSeed;
			if (num3 < 0f)
			{
				particleSystems[num].Play(false);
				particleSystems[num].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
	}
}
