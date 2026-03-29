using UnityEngine;

public class RewindParticleSystemSimple : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private float simulationTime;

	public float startTime = 2f;

	private float internalStartTime;

	private bool gameObjectDeactivated;

	public float simulationSpeed = 1f;

	public bool useFixedDeltaTime = true;

	public bool rewind = true;

	private void OnEnable()
	{
		bool flag = particleSystems == null;
		if (flag)
		{
			particleSystems = GetComponentsInChildren<ParticleSystem>(false);
		}
		simulationTime = 0f;
		if (flag || gameObjectDeactivated)
		{
			internalStartTime = startTime;
		}
		else
		{
			internalStartTime = particleSystems[0].time;
		}
		for (int num = particleSystems.Length - 1; num >= 0; num--)
		{
			particleSystems[num].Simulate(internalStartTime, false, false, useFixedDeltaTime);
		}
	}

	private void OnDisable()
	{
		particleSystems[0].Play(true);
		gameObjectDeactivated = !base.gameObject.activeInHierarchy;
	}

	private void Update()
	{
		simulationTime -= Time.deltaTime * simulationSpeed;
		float num = internalStartTime + simulationTime;
		particleSystems[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		for (int num2 = particleSystems.Length - 1; num2 >= 0; num2--)
		{
			bool useAutoRandomSeed = particleSystems[num2].useAutoRandomSeed;
			particleSystems[num2].useAutoRandomSeed = false;
			particleSystems[num2].Play(false);
			particleSystems[num2].Simulate(num, false, false, useFixedDeltaTime);
			particleSystems[num2].useAutoRandomSeed = useAutoRandomSeed;
			if (num < 0f)
			{
				particleSystems[num2].Play();
				particleSystems[num2].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
	}
}
