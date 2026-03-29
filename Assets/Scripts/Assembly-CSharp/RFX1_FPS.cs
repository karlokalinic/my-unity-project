using UnityEngine;

public class RFX1_FPS : MonoBehaviour
{
	public GUIStyle guiStyleHeader = new GUIStyle();

	private float timeleft;

	private float timeleft2;

	private const float updateTime = 0.5f;

	private float fps;

	private int frames;

	private void OnGUI()
	{
		GUI.Label(new Rect(0f, 0f, 30f, 30f), "FPS: " + (float)(int)fps / 0.5f, guiStyleHeader);
	}

	private void Update()
	{
		timeleft -= Time.deltaTime;
		frames++;
		if ((double)timeleft <= 0.0)
		{
			fps = frames;
			timeleft = 0.5f;
			frames = 0;
		}
	}
}
