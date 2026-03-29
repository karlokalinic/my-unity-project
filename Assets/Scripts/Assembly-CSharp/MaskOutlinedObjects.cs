using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MaskOutlinedObjects : MonoBehaviour
{
	[Range(1f, 5f)]
	public int thickness = 5;

	[Range(0f, 1f)]
	public float blending = 0.5f;

	protected Shader shaderRender;

	private Material m_Material;

	private Shader shaderMask;

	private GameObject maskCameraObj;

	private Camera maskCamera;

	private RenderTexture maskTexture;

	private Camera mainCamera;

	private Texture2D defaultTexture;

	protected Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = new Material(shaderRender);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material;
		}
	}

	private void Awake()
	{
		shaderMask = Shader.Find("3y3net/OutlineMask");
		Color color = new Color(0f, 0f, 0f, 0f);
		defaultTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
		defaultTexture.SetPixel(0, 0, color);
		defaultTexture.SetPixel(1, 0, color);
		defaultTexture.SetPixel(0, 1, color);
		defaultTexture.SetPixel(1, 1, color);
		defaultTexture.Apply();
		Shader.SetGlobalTexture("_SpriteMask", defaultTexture);
		shaderRender = Shader.Find("3y3net/OutlineRender");
	}

	private void OnEnable()
	{
		if (!shaderMask || !shaderMask.isSupported)
		{
			base.enabled = false;
			return;
		}
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (!shaderRender || !shaderRender.isSupported)
		{
			base.enabled = false;
			return;
		}
		mainCamera = GetComponent<Camera>();
		if (!maskCameraObj)
		{
			maskCameraObj = new GameObject("OutlineMaskCamera", typeof(Camera));
			maskCameraObj.hideFlags = HideFlags.HideAndDontSave;
			maskCamera = maskCameraObj.GetComponent<Camera>();
			maskCamera.enabled = false;
		}
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(maskCameraObj);
		if ((bool)m_Material)
		{
			Object.DestroyImmediate(m_Material);
		}
	}

	private void OnPreCull()
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			maskTexture = RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.ARGB32);
			maskCamera.CopyFrom(mainCamera);
			maskCamera.clearFlags = CameraClearFlags.Color;
			maskCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			maskCamera.renderingPath = RenderingPath.Forward;
			maskCamera.targetTexture = maskTexture;
			maskCamera.RenderWithShader(shaderMask, "RenderType");
			Shader.SetGlobalTexture("_OutlineTextureMask", maskTexture);
		}
	}

	private void OnPostRender()
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			RenderTexture.ReleaseTemporary(maskTexture);
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		material.SetFloat("_Thickness", thickness);
		material.SetFloat("_Blending", blending + 0.5f);
		Graphics.Blit(src, dest, material);
	}
}
