using UnityEngine;

public class RFX1_ProjectorSizeCurves : MonoBehaviour
{
	public AnimationCurve ProjectorSize = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private Projector projector;

	private void Awake()
	{
		projector = GetComponent<Projector>();
		projector.orthographicSize = ProjectorSize.Evaluate(0f);
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float orthographicSize = ProjectorSize.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			projector.orthographicSize = orthographicSize;
		}
		if (num >= GraphTimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
	}
}
