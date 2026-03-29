using UnityEngine;

public class RFX1_ActivateGravityAfterCollision : MonoBehaviour
{
	public RFX1_TransformMotion TransformMotion;

	public Vector2 Gravity = new Vector2(1f, 1f);

	private ParticleSystem ps;

	private ParticleSystem.MinMaxCurve startGravity;

	private bool isInitialized;

	private void OnEnable()
	{
		TransformMotion.CollisionEnter += TransformMotion_CollisionEnter;
		ps = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		if (!isInitialized)
		{
			isInitialized = true;
			startGravity = main.gravityModifier;
		}
		else
		{
			main.gravityModifier = startGravity;
		}
	}

	private void OnDisable()
	{
		TransformMotion.CollisionEnter -= TransformMotion_CollisionEnter;
	}

	private void TransformMotion_CollisionEnter(object sender, RFX1_TransformMotion.RFX1_CollisionInfo e)
	{
		ParticleSystem.MainModule main = ps.main;
		main.gravityModifier = new ParticleSystem.MinMaxCurve(Gravity.x, Gravity.y);
	}
}
