using UnityEngine;

namespace nightowl.HoloShaderPack
{
	public class ScreenShotTool : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.S))
			{
				ScreenCapture.CaptureScreenshot("4KScreenshot.png");
			}
		}

		private void OnGUI()
		{
		}
	}
}
