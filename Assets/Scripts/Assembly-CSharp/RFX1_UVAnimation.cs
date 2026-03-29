using UnityEngine;

public class RFX1_UVAnimation : MonoBehaviour
{
	public int TilesX = 4;

	public int TilesY = 4;

	public int FPS = 30;

	public float StartDelay;

	public bool IsLoop = true;

	public bool IsReverse;

	public bool IsInterpolateFrames;

	public bool IsParticleSystemTrail;

	public RFX1_TextureShaderProperties[] TextureNames = new RFX1_TextureShaderProperties[1];

	public AnimationCurve FrameOverTime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	private Renderer currentRenderer;

	private Projector projector;

	private Material instanceMaterial;

	private float animationStartTime;

	private bool canUpdate;

	private int previousIndex;

	private int totalFrames;

	private float currentInterpolatedTime;

	private int currentIndex;

	private Vector2 size;

	private bool isInitialized;

	private bool startDelayIsBroken;

	private ParticleSystemRenderer pr;

	private void OnEnable()
	{
		if (isInitialized)
		{
			InitDefaultVariables();
		}
	}

	private void Start()
	{
		InitDefaultVariables();
		isInitialized = true;
	}

	private void Update()
	{
		if (startDelayIsBroken)
		{
			ManualUpdate();
		}
	}

	private void ManualUpdate()
	{
		if (canUpdate)
		{
			UpdateMaterial();
			SetSpriteAnimation();
			if (IsInterpolateFrames)
			{
				SetSpriteAnimationIterpolated();
			}
		}
	}

	private void StartDelayFunc()
	{
		startDelayIsBroken = true;
		animationStartTime = Time.time;
	}

	private void InitDefaultVariables()
	{
		InitializeMaterial();
		totalFrames = TilesX * TilesY;
		previousIndex = 0;
		canUpdate = true;
		Vector3 zero = Vector3.zero;
		size = new Vector2(1f / (float)TilesX, 1f / (float)TilesY);
		animationStartTime = Time.time;
		if (StartDelay > 1E-05f)
		{
			startDelayIsBroken = false;
			Invoke("StartDelayFunc", StartDelay);
		}
		else
		{
			startDelayIsBroken = true;
		}
		if (instanceMaterial != null)
		{
			RFX1_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX1_TextureShaderProperties rFX1_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX1_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX1_TextureShaderProperties.ToString(), zero);
			}
		}
	}

	private void InitializeMaterial()
	{
		if (IsParticleSystemTrail)
		{
			pr = GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
			currentRenderer = pr;
			instanceMaterial = pr.trailMaterial;
			if (!instanceMaterial.name.EndsWith("(Instance)"))
			{
				instanceMaterial = new Material(instanceMaterial)
				{
					name = instanceMaterial.name + " (Instance)"
				};
			}
			pr.trailMaterial = instanceMaterial;
			return;
		}
		currentRenderer = GetComponent<Renderer>();
		if (currentRenderer == null)
		{
			projector = GetComponent<Projector>();
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void UpdateMaterial()
	{
		if (currentRenderer == null)
		{
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else if (!IsParticleSystemTrail)
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void SetSpriteAnimation()
	{
		int num = (int)((Time.time - animationStartTime) * (float)FPS);
		num %= totalFrames;
		if (!IsLoop && num < previousIndex)
		{
			canUpdate = false;
			return;
		}
		if (IsInterpolateFrames && num != previousIndex)
		{
			currentInterpolatedTime = 0f;
		}
		previousIndex = num;
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 value = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			RFX1_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				RFX1_TextureShaderProperties rFX1_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(rFX1_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(rFX1_TextureShaderProperties.ToString(), value);
			}
		}
	}

	private void SetSpriteAnimationIterpolated()
	{
		currentInterpolatedTime += Time.deltaTime;
		int num = previousIndex + 1;
		if (num == totalFrames)
		{
			num = previousIndex;
		}
		if (IsReverse)
		{
			num = totalFrames - num - 1;
		}
		int num2 = num % TilesX;
		int num3 = num / TilesX;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 vector = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			instanceMaterial.SetVector("_Tex_NextFrame", new Vector4(size.x, size.y, vector.x, vector.y));
			instanceMaterial.SetFloat("InterpolationValue", Mathf.Clamp01(currentInterpolatedTime * (float)FPS));
			instanceMaterial.SetVector("_MainTex_NextFrame", new Vector4(size.x, size.y, vector.x, vector.y));
		}
	}
}
