using UnityEngine;

public class RFX1_ShaderFloatCurve : MonoBehaviour
{
	public RFX1_ShaderProperties ShaderFloatProperty = RFX1_ShaderProperties._Cutoff;

	public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public bool IsLoop;

	public bool UseSharedMaterial;

	private bool canUpdate;

	private float startTime;

	private int propertyID;

	private string shaderProperty;

	private bool isInitialized;

	private MaterialPropertyBlock props;

	private Renderer rend;

	private void Awake()
	{
		if (props == null)
		{
			props = new MaterialPropertyBlock();
		}
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
		}
		shaderProperty = ShaderFloatProperty.ToString();
		propertyID = Shader.PropertyToID(shaderProperty);
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
		rend.GetPropertyBlock(props);
		float value = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
		props.SetFloat(propertyID, value);
		rend.SetPropertyBlock(props);
	}

	private void Update()
	{
		rend.GetPropertyBlock(props);
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float value = FloatCurve.Evaluate(num / GraphTimeMultiplier) * GraphIntensityMultiplier;
			props.SetFloat(propertyID, value);
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
		rend.SetPropertyBlock(props);
	}
}
