using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
	public GameObject[] particles;

	public int maxButtons = 10;

	public bool showInfo;

	public string removeTextFromButton;

	private int page;

	private int pages;

	private string currentPSInfo;

	private GameObject currentPS;

	public void Start()
	{
		pages = (int)Mathf.Ceil((particles.Length - 1) / maxButtons);
	}

	public void OnGUI()
	{
		Time.timeScale = GUI.VerticalSlider(new Rect(185f, 50f, 20f, 150f), Time.timeScale, 2f, 0f);
		if (particles.Length > maxButtons)
		{
			if (GUI.Button(new Rect(20f, (maxButtons + 1) * 18, 75f, 18f), "Prev"))
			{
				if (page > 0)
				{
					page--;
				}
				else
				{
					page = pages;
				}
			}
			if (GUI.Button(new Rect(95f, (maxButtons + 1) * 18, 75f, 18f), "Next"))
			{
				if (page < pages)
				{
					page++;
				}
				else
				{
					page = 0;
				}
			}
			GUI.Label(new Rect(60f, (maxButtons + 2) * 18, 150f, 22f), "Page" + (page + 1) + " / " + (pages + 1));
		}
		showInfo = GUI.Toggle(new Rect(185f, 20f, 75f, 25f), showInfo, "Info");
		if (showInfo)
		{
			GUI.Label(new Rect(250f, 20f, 500f, 500f), currentPSInfo);
		}
		int num = particles.Length - page * maxButtons;
		if (num > maxButtons)
		{
			num = maxButtons;
		}
		for (int i = 0; i < num; i++)
		{
			string text = particles[i + page * maxButtons].transform.name;
			text = text.Replace(removeTextFromButton, string.Empty);
			if (GUI.Button(new Rect(20f, i * 18 + 18, 150f, 18f), text))
			{
				if (currentPS != null)
				{
					Object.Destroy(currentPS);
				}
				GameObject gameObject = (currentPS = Object.Instantiate(particles[i + page * maxButtons]));
				PlayPS(gameObject.GetComponent<ParticleSystem>(), i + page * maxButtons + 1);
				InfoPS(gameObject.GetComponent<ParticleSystem>(), i + page * maxButtons + 1);
			}
		}
	}

	public void PlayPS(ParticleSystem _ps, int _nr)
	{
		Time.timeScale = 1f;
		_ps.Play();
	}

	public void InfoPS(ParticleSystem _ps, int _nr)
	{
		currentPSInfo = "System: " + _nr + "/" + particles.Length + "\nName: " + _ps.gameObject.name + "\n\nMain PS Sub Particles: " + _ps.transform.childCount + "\nMain PS Materials: " + _ps.GetComponent<Renderer>().materials.Length + "\nMain PS Shader: " + _ps.GetComponent<Renderer>().material.shader.name;
		if (_ps.GetComponent<Renderer>().materials.Length >= 2)
		{
			currentPSInfo += "\n\n *Plasma not mobile optimized*";
		}
		currentPSInfo += "\n\n Use mouse wheel to zoom, click and hold to rotate";
		currentPSInfo = currentPSInfo.Replace("(Clone)", string.Empty);
	}
}
