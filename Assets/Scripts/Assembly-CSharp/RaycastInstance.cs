using UnityEngine;

public class RaycastInstance : MonoBehaviour
{
	public Camera Cam;

	public GameObject[] Prefabs;

	private int Prefab;

	private Ray RayMouse;

	private GameObject Instance;

	private float windowDpi;

	private float buttonSaver;

	private void Start()
	{
		if (Screen.dpi < 1f)
		{
			windowDpi = 1f;
		}
		if (Screen.dpi < 200f)
		{
			windowDpi = 1f;
		}
		else
		{
			windowDpi = Screen.dpi / 200f;
		}
		Counter(0);
	}

	private void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			if (Cam != null)
			{
				Vector3 mousePosition = Input.mousePosition;
				RayMouse = Cam.ScreenPointToRay(mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast(RayMouse.origin, RayMouse.direction, out hitInfo, 40f))
				{
					Instance = Object.Instantiate(Prefabs[Prefab]);
					Instance.transform.position = hitInfo.point + hitInfo.normal * 0.01f;
					Object.Destroy(Instance, 1.5f);
				}
			}
			else
			{
				Debug.Log("No camera");
			}
		}
		if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0f) && buttonSaver >= 0.4f)
		{
			buttonSaver = 0f;
			Counter(-1);
		}
		if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0f) && buttonSaver >= 0.4f)
		{
			buttonSaver = 0f;
			Counter(1);
		}
		buttonSaver += Time.deltaTime;
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f * windowDpi, 5f * windowDpi, 400f * windowDpi, 20f * windowDpi), "Use the keyboard buttons A/<- and D/-> to change prefabs!");
		GUI.Label(new Rect(10f * windowDpi, 20f * windowDpi, 400f * windowDpi, 20f * windowDpi), "Use left mouse button for instancing!");
	}

	private void Counter(int count)
	{
		Prefab += count;
		if (Prefab > Prefabs.Length - 1)
		{
			Prefab = 0;
		}
		else if (Prefab < 0)
		{
			Prefab = Prefabs.Length - 1;
		}
	}
}
