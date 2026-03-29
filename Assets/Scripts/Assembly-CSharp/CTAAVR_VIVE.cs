using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CTAAVR_Velocity_OPENVR))]
[AddComponentMenu("Livenda Effects/CTAAVR_VIVE")]
public class CTAAVR_VIVE : MonoBehaviour
{
	public bool CTAA_Enabled = true;

	[Header("CTAA Settings")]
	[Range(1f, 4f)]
	public float TemporalEdgePower = 1.5f;

	[Space(5f)]
	[Range(0f, 0.5f)]
	public float TemporalJitterScale;

	[Space(5f)]
	public Camera.StereoscopicEye VRCameraEYE;

	private float TemporalQuality = 1.5f;

	private float jitterScale;

	private int forwardMode;

	private CTAAVR_Velocity_OPENVR _velocity;

	private Material mat_txaa;

	private Material mat_enhance;

	private RenderTexture rtAccum0;

	private RenderTexture rtAccum1;

	private RenderTexture txaaOut;

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

	private void Awake()
	{
		_velocity = GetComponent<CTAAVR_Velocity_OPENVR>();
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

	private void OnEnable()
	{
		firstFrame = true;
		swap = true;
		CreateMaterials();
		Camera component = GetComponent<Camera>();
		if (component.actualRenderingPath == RenderingPath.Forward)
		{
			forwardMode = 1;
		}
		else
		{
			forwardMode = 0;
		}
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(rtAccum0);
		rtAccum0 = null;
		Object.DestroyImmediate(rtAccum1);
		rtAccum1 = null;
		Object.DestroyImmediate(txaaOut);
		txaaOut = null;
		DestroyMaterial(mat_txaa);
		DestroyMaterial(mat_enhance);
	}

	private void CreateMaterials()
	{
		if (mat_txaa == null)
		{
			mat_txaa = CreateMaterial("Hidden/CTAAv301VRVIVE");
		}
		if (mat_enhance == null)
		{
			mat_enhance = CreateMaterial("Hidden/AdaptiveEnhanceVR_VIVE");
		}
	}

	private void SetCTAA_Parameters()
	{
		TemporalQuality = TemporalEdgePower;
		jitterScale = TemporalJitterScale;
	}

	private void Start()
	{
		CreateMaterials();
		Camera component = GetComponent<Camera>();
		if (component.actualRenderingPath == RenderingPath.Forward)
		{
			forwardMode = 1;
		}
		else
		{
			forwardMode = 0;
		}
		component.depthTextureMode = DepthTextureMode.Depth;
		SetCTAA_Parameters();
		StartCoroutine(fixCam());
	}

	private IEnumerator fixCam()
	{
		Camera _camera = GetComponent<Camera>();
		if (_camera.actualRenderingPath == RenderingPath.Forward)
		{
			_camera.renderingPath = RenderingPath.DeferredShading;
			yield return new WaitForSeconds(0.5f);
			_camera.renderingPath = RenderingPath.Forward;
		}
		yield return new WaitForSeconds(0.1f);
	}

	private void OnPreCull()
	{
		jitterCam();
	}

	private void jitterCam()
	{
		Camera component = GetComponent<Camera>();
		GetComponent<Camera>().ResetStereoProjectionMatrices();
		Matrix4x4 stereoProjectionMatrix = component.GetStereoProjectionMatrix(VRCameraEYE);
		float num = x_jit[frameCounter] * jitterScale;
		float num2 = y_jit[frameCounter] * jitterScale;
		stereoProjectionMatrix.m02 += (num * 2f - 1f) / GetComponent<Camera>().pixelRect.width;
		stereoProjectionMatrix.m12 += (num2 * 2f - 1f) / GetComponent<Camera>().pixelRect.height;
		frameCounter++;
		frameCounter %= 8;
		component.SetStereoProjectionMatrix(VRCameraEYE, stereoProjectionMatrix);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		SetCTAA_Parameters();
		CreateMaterials();
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
		_velocity.RenderVel();
		if (CTAA_Enabled)
		{
			mat_txaa.SetFloat("_RenderPath", forwardMode);
			if (firstFrame)
			{
				Graphics.Blit(source, rtAccum0);
				firstFrame = false;
			}
			mat_txaa.SetTexture("_Motion0", _velocity.velocityBuffer);
			float y = TemporalQuality;
			mat_txaa.SetVector("_ControlParams", new Vector4(0f, y, 0f, 0f));
			if (swap)
			{
				mat_txaa.SetTexture("_Accum", rtAccum0);
				Graphics.Blit(source, rtAccum1, mat_txaa);
				Graphics.Blit(rtAccum1, destination);
			}
			else
			{
				mat_txaa.SetTexture("_Accum", rtAccum1);
				Graphics.Blit(source, rtAccum0, mat_txaa);
				Graphics.Blit(rtAccum0, destination);
			}
		}
		else
		{
			Graphics.Blit(source, destination);
		}
		swap = !swap;
	}
}
