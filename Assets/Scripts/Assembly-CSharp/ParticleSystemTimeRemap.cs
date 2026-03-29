using UnityEngine;

public class ParticleSystemTimeRemap : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private float[] startTimes;

	private float[] simulationTimes;

	public float startTime = 2f;

	public float simulationSpeedScale = 1f;

	public bool useFixedDeltaTime = true;

	private bool gameObjectDeactivated;

	public bool reverseSimulation;

	private float elapsedTime;

	public AnimationCurve simulationSpeedOverTime = AnimationCurve.Linear(0f, 1f, 5f, 1f);

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
		if (!reverseSimulation)
		{
			particleSystems[0].Play(true);
		}
	}

	private void OnDisable()
	{
		particleSystems[0].Play(true);
		gameObjectDeactivated = !base.gameObject.activeInHierarchy;
	}

	private void Update()
	{
		elapsedTime += Time.deltaTime;
		float simulationSpeed = simulationSpeedScale * simulationSpeedOverTime.Evaluate(elapsedTime);
		if (!reverseSimulation)
		{
			for (int i = 0; i < particleSystems.Length; i++)
			{
				ParticleSystem.MainModule main = particleSystems[i].main;
				main.simulationSpeed = simulationSpeed;
			}
			return;
		}
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
