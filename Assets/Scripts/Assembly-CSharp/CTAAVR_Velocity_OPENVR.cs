using System;
using System.Collections.Generic;
using UnityEngine;

public class CTAAVR_Velocity_OPENVR : MonoBehaviour
{
	private const RenderTextureFormat velocityFormat = RenderTextureFormat.RGFloat;

	public Shader velocityShader;

	public Camera.StereoscopicEye VRCameraEYE;

	private Material velocityMaterial;

	private Matrix4x4? velocityViewMatrix;

	[NonSerialized]
	[HideInInspector]
	public RenderTexture velocityBuffer;

	private float timeScaleNextFrame;

	private Camera _camera;

	public float timeScale { get; private set; }

	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void Start()
	{
		timeScaleNextFrame = Time.timeScale;
	}

	private void EnsureMaterial(ref Material material, Shader shader)
	{
		if (shader != null)
		{
			if (material == null || material.shader != shader)
			{
				material = new Material(shader);
			}
			if (material != null)
			{
				material.hideFlags = HideFlags.DontSave;
			}
		}
		else
		{
			Debug.LogWarning("missing shader", this);
		}
	}

	private void EnsureKeyword(Material material, string name, bool enabled)
	{
		if (enabled != material.IsKeywordEnabled(name))
		{
			if (enabled)
			{
				material.EnableKeyword(name);
			}
			else
			{
				material.DisableKeyword(name);
			}
		}
	}

	private void EnsureRenderTarget(ref RenderTexture rt, int width, int height, RenderTextureFormat format, FilterMode filterMode, int depthBits = 0)
	{
		if (rt != null && (rt.width != width || rt.height != height || rt.format != format || rt.filterMode != filterMode))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
		if (rt == null)
		{
			rt = RenderTexture.GetTemporary(width, height, depthBits, format);
			rt.filterMode = filterMode;
			rt.wrapMode = TextureWrapMode.Clamp;
		}
	}

	private void FullScreenQuad()
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.Vertex3(0f, 0f, 0f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.Vertex3(1f, 0f, 0f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.Vertex3(1f, 1f, 0f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}

	private Matrix4x4 GetPerspectiveProjection(float left, float right, float bottom, float top, float near, float far)
	{
		float value = 2f * near / (right - left);
		float value2 = 2f * near / (top - bottom);
		float value3 = (right + left) / (right - left);
		float value4 = (top + bottom) / (top - bottom);
		float value5 = (0f - (far + near)) / (far - near);
		float value6 = (0f - 2f * far * near) / (far - near);
		float value7 = -1f;
		Matrix4x4 result = default(Matrix4x4);
		result[0, 0] = value;
		result[0, 1] = 0f;
		result[0, 2] = value3;
		result[0, 3] = 0f;
		result[1, 0] = 0f;
		result[1, 1] = value2;
		result[1, 2] = value4;
		result[1, 3] = 0f;
		result[2, 0] = 0f;
		result[2, 1] = 0f;
		result[2, 2] = value5;
		result[2, 3] = value6;
		result[3, 0] = 0f;
		result[3, 1] = 0f;
		result[3, 2] = value7;
		result[3, 3] = 0f;
		return result;
	}

	private Matrix4x4 GetPerspectiveProjection(Camera camera)
	{
		return GetPerspectiveProjection(camera, 0f, 0f);
	}

	private Matrix4x4 GetPerspectiveProjection(Camera camera, float texelOffsetX, float texelOffsetY)
	{
		if (camera == null)
		{
			return Matrix4x4.identity;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float num5 = num3 * texelOffsetX;
		float num6 = num4 * texelOffsetY;
		float farClipPlane = camera.farClipPlane;
		float nearClipPlane = camera.nearClipPlane;
		float left = (num5 - num2) * nearClipPlane;
		float right = (num5 + num2) * nearClipPlane;
		float bottom = (num6 - num) * nearClipPlane;
		float top = (num6 + num) * nearClipPlane;
		return GetPerspectiveProjection(left, right, bottom, top, nearClipPlane, farClipPlane);
	}

	private Vector4 GetPerspectiveProjectionCornerRay(Camera camera)
	{
		return GetPerspectiveProjectionCornerRay(camera, 0f, 0f);
	}

	private Vector4 GetPerspectiveProjectionCornerRay(Camera camera, float texelOffsetX, float texelOffsetY)
	{
		if (camera == null)
		{
			return Vector4.zero;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float z = num3 * texelOffsetX;
		float w = num4 * texelOffsetY;
		return new Vector4(num2, num, z, w);
	}

	public void RenderVel()
	{
		Matrix4x4 projectionMatrix = GetComponent<Camera>().projectionMatrix;
		GetComponent<Camera>().ResetProjectionMatrix();
		GetComponent<Camera>().ResetStereoProjectionMatrices();
		EnsureMaterial(ref velocityMaterial, velocityShader);
		if (_camera.orthographic || _camera.depthTextureMode == DepthTextureMode.None || velocityMaterial == null)
		{
			if (_camera.depthTextureMode == DepthTextureMode.None)
			{
				_camera.depthTextureMode = DepthTextureMode.Depth;
			}
			return;
		}
		timeScale = timeScaleNextFrame;
		timeScaleNextFrame = ((Time.timeScale != 0f) ? Time.timeScale : timeScaleNextFrame);
		int pixelWidth = _camera.pixelWidth;
		int pixelHeight = _camera.pixelHeight;
		EnsureRenderTarget(ref velocityBuffer, pixelWidth, pixelHeight, RenderTextureFormat.RGFloat, FilterMode.Point, 24);
		Matrix4x4 projectionMatrix2 = _camera.projectionMatrix;
		Matrix4x4 worldToCameraMatrix = _camera.worldToCameraMatrix;
		Matrix4x4 value = projectionMatrix2 * worldToCameraMatrix;
		Matrix4x4? matrix4x = velocityViewMatrix;
		if (!matrix4x.HasValue)
		{
			velocityViewMatrix = worldToCameraMatrix;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = velocityBuffer;
		GL.Clear(true, true, Color.black);
		velocityMaterial.SetVector("_Corner", GetPerspectiveProjectionCornerRay(_camera));
		velocityMaterial.SetMatrix("_CurrV", worldToCameraMatrix);
		velocityMaterial.SetMatrix("_CurrVP", value);
		velocityMaterial.SetMatrix("_PrevVP", projectionMatrix2 * velocityViewMatrix.Value);
		velocityMaterial.SetPass(0);
		FullScreenQuad();
		List<DynamicObjectTag> activeObjects = DynamicObjectTag.activeObjects;
		int i = 0;
		for (int count = activeObjects.Count; i != count; i++)
		{
			DynamicObjectTag dynamicObjectTag = activeObjects[i];
			if (dynamicObjectTag != null && !dynamicObjectTag.sleeping && dynamicObjectTag.mesh != null)
			{
				velocityMaterial.SetMatrix("_CurrM", dynamicObjectTag.localToWorldCurr);
				velocityMaterial.SetMatrix("_PrevM", dynamicObjectTag.localToWorldPrev);
				velocityMaterial.SetPass((!dynamicObjectTag.useSkinnedMesh) ? 1 : 2);
				for (int j = 0; j != dynamicObjectTag.mesh.subMeshCount; j++)
				{
					Graphics.DrawMeshNow(dynamicObjectTag.mesh, Matrix4x4.identity, j);
				}
			}
		}
		RenderTexture.active = active;
		velocityViewMatrix = worldToCameraMatrix;
		GetComponent<Camera>().SetStereoProjectionMatrix(VRCameraEYE, projectionMatrix);
	}
}
