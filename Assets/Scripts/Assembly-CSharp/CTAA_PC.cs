using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/LIVENDA/CTAA_PC")]
public class CTAA_PC : MonoBehaviour
{
	[Space(5f)]
	public bool CTAA_Enabled = true;

	[Header("CTAA Settings")]
	[Range(3f, 16f)]
	public int TemporalStability = 7;

	[Space(5f)]
	[Range(0f, 16f)]
	public float TemporalContrast = 3.3f;

	[Space(5f)]
	[Range(0f, 0.5f)]
	public float AdaptiveEnhance = 0.2f;

	[Space(5f)]
	[Range(0f, 0.5f)]
	public float TemporalJitterScale = 0.5f;

	private bool PreEnhanceEnabled = true;

	private float preEnhanceStrength = 1f;

	private float preEnhanceClamp = 0.005f;

	private float AdaptiveResolve = 3000f;

	private float jitterScale = 1f;

	private Material ctaaMat;

	private Material mat_enhance;

	private RenderTexture rtAccum0;

	private RenderTexture rtAccum1;

	private RenderTexture txaaOut;

	private RenderTexture afterPreEnhace;

	private bool firstFrame;

	private bool swap;

	private int frameCounter;

	private float[] x_jit = new float[8] { 0.5f, -0.25f, 0.75f, -0.125f, 0.625f, 0.575f, -0.875f, 0.0625f };

	private float[] y_jit = new float[8]
	{
		0.3333333f,
		-2f / 3f,
		0.5111111f,
		0.4444444f,
		-7f / 9f,
		0.1222222f,
		-5f / 9f,
		8f / 9f
	};

	private void SetCTAA_Parameters()
	{
		PreEnhanceEnabled = (((double)AdaptiveEnhance > 0.01) ? true : false);
		preEnhanceStrength = Mathf.Lerp(0.2f, 2f, AdaptiveEnhance);
		preEnhanceClamp = Mathf.Lerp(0.005f, 0.12f, AdaptiveEnhance);
		jitterScale = TemporalJitterScale;
		AdaptiveResolve = 3000f;
	}

	private static Material CreateMaterial(string shadername)
	{
		if (string.IsNullOrEmpty(shadername))
		{
			return null;
		}
		Material material = new Material(Shader.Find(shadername));
		material.hideFlags = HideFlags.HideAndDontSave;
		return material;
	}

	private static void DestroyMaterial(Material mat)
	{
		if (mat != null)
		{
			Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	private void Awake()
	{
		if (ctaaMat == null)
		{
			ctaaMat = CreateMaterial("Hidden/CTAA_PC");
		}
		if (mat_enhance == null)
		{
			mat_enhance = CreateMaterial("Hidden/CTAA_Enhance_PC");
		}
		firstFrame = true;
		swap = true;
		frameCounter = 0;
		SetCTAA_Parameters();
	}

	private void OnEnable()
	{
		Camera component = GetComponent<Camera>();
		component.depthTextureMode |= DepthTextureMode.DepthNormals;
		component.depthTextureMode |= DepthTextureMode.Depth;
		component.depthTextureMode |= DepthTextureMode.MotionVectors;
	}

	private void OnDisable()
	{
		DestroyMaterial(ctaaMat);
		DestroyMaterial(mat_enhance);
		Object.DestroyImmediate(rtAccum0);
		rtAccum0 = null;
		Object.DestroyImmediate(rtAccum1);
		rtAccum1 = null;
		Object.DestroyImmediate(txaaOut);
		txaaOut = null;
		Object.DestroyImmediate(afterPreEnhace);
		afterPreEnhace = null;
	}

	private void OnPreCull()
	{
		jitterCam();
	}

	private void jitterCam()
	{
		GetComponent<Camera>().ResetWorldToCameraMatrix();
		GetComponent<Camera>().ResetProjectionMatrix();
		GetComponent<Camera>().nonJitteredProjectionMatrix = GetComponent<Camera>().projectionMatrix;
		Matrix4x4 projectionMatrix = GetComponent<Camera>().projectionMatrix;
		float num = x_jit[frameCounter] * jitterScale;
		float num2 = y_jit[frameCounter] * jitterScale;
		projectionMatrix.m02 += (num * 2f - 1f) / GetComponent<Camera>().pixelRect.width;
		projectionMatrix.m12 += (num2 * 2f - 1f) / GetComponent<Camera>().pixelRect.height;
		frameCounter++;
		frameCounter %= 8;
		GetComponent<Camera>().projectionMatrix = projectionMatrix;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (CTAA_Enabled)
		{
			SetCTAA_Parameters();
		}
		else
		{
			jitterScale = 0f;
		}
		if (rtAccum0 == null || rtAccum0.width != source.width || rtAccum0.height != source.height)
		{
			Object.DestroyImmediate(rtAccum0);
			rtAccum0 = new RenderTexture(source.width, source.height, 0, source.format);
			rtAccum0.hideFlags = HideFlags.HideAndDontSave;
			rtAccum0.filterMode = FilterMode.Bilinear;
			rtAccum0.wrapMode = TextureWrapMode.Clamp;
		}
		if (rtAccum1 == null || rtAccum1.width != source.width || rtAccum1.height != source.height)
		{
			Object.DestroyImmediate(rtAccum1);
			rtAccum1 = new RenderTexture(source.width, source.height, 0, source.format);
			rtAccum1.hideFlags = HideFlags.HideAndDontSave;
			rtAccum1.filterMode = FilterMode.Bilinear;
			rtAccum1.wrapMode = TextureWrapMode.Clamp;
		}
		if (txaaOut == null || txaaOut.width != source.width || txaaOut.height != source.height)
		{
			Object.DestroyImmediate(txaaOut);
			txaaOut = new RenderTexture(source.width, source.height, 0, source.format);
			txaaOut.hideFlags = HideFlags.HideAndDontSave;
			txaaOut.filterMode = FilterMode.Bilinear;
			txaaOut.wrapMode = TextureWrapMode.Clamp;
		}
		if (afterPreEnhace == null || afterPreEnhace.width != source.width || afterPreEnhace.height != source.height)
		{
			Object.DestroyImmediate(afterPreEnhace);
			afterPreEnhace = new RenderTexture(source.width, source.height, 0, source.format);
			afterPreEnhace.hideFlags = HideFlags.HideAndDontSave;
			afterPreEnhace.filterMode = source.filterMode;
			afterPreEnhace.wrapMode = TextureWrapMode.Clamp;
		}
		if (PreEnhanceEnabled)
		{
			mat_enhance.SetFloat("_AEXCTAA", 1f / (float)Screen.width);
			mat_enhance.SetFloat("_AEYCTAA", 1f / (float)Screen.height);
			mat_enhance.SetFloat("_AESCTAA", preEnhanceStrength);
			mat_enhance.SetFloat("_AEMAXCTAA", preEnhanceClamp);
			Graphics.Blit(source, afterPreEnhace, mat_enhance, 1);
		}
		else
		{
			Graphics.Blit(source, afterPreEnhace);
		}
		if (CTAA_Enabled)
		{
			if (firstFrame)
			{
				Graphics.Blit(afterPreEnhace, rtAccum0);
				firstFrame = false;
			}
			ctaaMat.SetFloat("_AdaptiveResolve", AdaptiveResolve);
			ctaaMat.SetVector("_ControlParams", new Vector4(0f, TemporalStability, TemporalContrast, 0f));
			if (swap)
			{
				ctaaMat.SetTexture("_Accum", rtAccum0);
				Graphics.Blit(afterPreEnhace, rtAccum1, ctaaMat);
				Graphics.Blit(rtAccum1, txaaOut);
			}
			else
			{
				ctaaMat.SetTexture("_Accum", rtAccum1);
				Graphics.Blit(afterPreEnhace, rtAccum0, ctaaMat);
				Graphics.Blit(rtAccum0, txaaOut);
			}
			Graphics.Blit(txaaOut, destination);
		}
		else
		{
			Graphics.Blit(afterPreEnhace, destination);
		}
		swap = !swap;
	}
}
