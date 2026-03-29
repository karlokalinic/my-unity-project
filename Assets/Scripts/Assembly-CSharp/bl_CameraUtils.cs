using UnityEngine;

public static class bl_CameraUtils
{
	public static bool GetCursorState
	{
		get
		{
			return Screen.lockCursor;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public static void LockCursor(bool mLock)
	{
		Screen.lockCursor = mLock;
	}
}
