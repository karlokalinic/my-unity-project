using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CTAAVRJitter_Oculus : MonoBehaviour
{
	public enum Pattern
	{
		Halton_2_3_X8 = 0
	}

	private static float[] points_Halton_2_3_x8;

	private static bool _initialized;

	private Vector3 focalMotionPos = Vector3.zero;

	private Vector3 focalMotionDir = Vector3.right;

	private Pattern pattern;

	[Range(0.25f, 0.65f)]
	public float jitterScale = 0.5f;

	private Vector4 activeSample = Vector4.zero;

	private int activeIndex = -1;

	public Camera.StereoscopicEye VRCameraEYE;

	static CTAAVRJitter_Oculus()
	{
		points_Halton_2_3_x8 = new float[16];
		_initialized = false;
		if (!_initialized)
		{
			_initialized = true;
			InitializeHalton_2_3(points_Halton_2_3_x8);
		}
	}

	private static float HaltonSeq(int prime, int index = 1)
	{
		float num = 0f;
		float num2 = 1f;
		for (int num3 = index; num3 > 0; num3 = (int)Mathf.Floor((float)num3 / (float)prime))
		{
			num2 /= (float)prime;
			num += num2 * (float)(num3 % prime);
		}
		return num;
	}

	private static void InitializeHalton_2_3(float[] seq)
	{
		int i = 0;
		for (int num = seq.Length / 2; i != num; i++)
		{
			float num2 = HaltonSeq(2, i + 1) - 0.5f;
			float num3 = HaltonSeq(3, i + 1) - 0.5f;
			seq[2 * i] = num2;
			seq[2 * i + 1] = num3;
		}
	}

	private static float[] AccessPointData(Pattern pattern)
	{
		if (pattern == Pattern.Halton_2_3_X8)
		{
			return points_Halton_2_3_x8;
		}
		Debug.LogError("missing point distribution");
		return points_Halton_2_3_x8;
	}

	public static int AccessLength(Pattern pattern)
	{
		return AccessPointData(pattern).Length / 2;
	}

	public Vector2 Sample(Pattern pattern, int index)
	{
		float[] array = AccessPointData(pattern);
		int num = array.Length / 2;
		int num2 = index % num;
		float x = jitterScale * array[2 * num2];
		float y = jitterScale * array[2 * num2 + 1];
		return new Vector2(x, y);
	}

	public float getActiveSampleX()
	{
		return activeSample.x;
	}

	public float getActiveSampleY()
	{
		return activeSample.y;
	}

	public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward = default(Vector3);
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;
		Vector3 upwards = default(Vector3);
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;
		return Quaternion.LookRotation(forward, upwards);
	}

	public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 result = default(Vector3);
		result.x = matrix.m03;
		result.y = matrix.m13;
		result.z = matrix.m23;
		return result;
	}

	private Vector3 WithZ(Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
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

	private Matrix4x4 GetPerspectiveProjection(Camera camera, float tsOXp, float tsOYp)
	{
		if (camera == null)
		{
			return Matrix4x4.identity;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float num5 = num3 * tsOXp;
		float num6 = num4 * tsOYp;
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

	private Vector4 GetPerspectiveProjectionCornerRay(Camera camera, float tsOXp, float tsOYp)
	{
		if (camera == null)
		{
			return Vector4.zero;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float z = num3 * tsOXp;
		float w = num4 * tsOYp;
		return new Vector4(num2, num, z, w);
	}

	private void OnPreRender()
	{
		Camera component = GetComponent<Camera>();
		if (component != null && !component.orthographic)
		{
			Vector3 vector = focalMotionPos;
			Vector3 vector2 = component.transform.TransformVector(component.nearClipPlane * Vector3.forward);
			Vector3 vector3 = component.worldToCameraMatrix * vector;
			Vector3 vector4 = component.worldToCameraMatrix * vector2;
			Vector3 vector5 = WithZ(vector4 - vector3, 0f);
			float magnitude = vector5.magnitude;
			if (magnitude != 0f)
			{
				Vector3 b = vector5 / magnitude;
				if (b.sqrMagnitude != 0f)
				{
					focalMotionPos = vector2;
					focalMotionDir = Vector3.Slerp(focalMotionDir, b, 0.2f);
				}
			}
			activeIndex++;
			activeIndex %= AccessLength(pattern);
			Vector2 vector6 = Sample(pattern, activeIndex);
			activeSample.z = activeSample.x;
			activeSample.w = activeSample.y;
			activeSample.x = vector6.x;
			activeSample.y = vector6.y;
			Matrix4x4 perspectiveProjection = GetPerspectiveProjection(component, vector6.x, vector6.y);
			component.SetStereoProjectionMatrix(VRCameraEYE, perspectiveProjection);
		}
		else
		{
			activeSample = Vector4.zero;
			activeIndex = -1;
		}
	}

	private void OnDisable()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.ResetProjectionMatrix();
		}
		activeSample = Vector4.zero;
		activeIndex = -1;
	}
}
