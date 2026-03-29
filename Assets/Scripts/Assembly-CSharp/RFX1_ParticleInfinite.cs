using UnityEngine;

public class RFX1_ParticleInfinite : MonoBehaviour
{
	public float Delay = 3f;

	private ParticleSystem ps;

	private ParticleSystem.MainModule main;

	private float oldSimulation;

	private void OnEnable()
	{
		if (ps == null)
		{
			ps = GetComponent<ParticleSystem>();
			main = ps.main;
			oldSimulation = main.simulationSpeed;
		}
		else
		{
			main.simulationSpeed = oldSimulation;
		}
		CancelInvoke("UpdateParticles");
		Invoke("UpdateParticles", Delay);
	}

	private void UpdateParticles()
	{
		main.simulationSpeed = 0f;
	}
}
