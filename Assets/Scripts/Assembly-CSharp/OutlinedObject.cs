using System.Collections.Generic;
using UnityEngine;

public class OutlinedObject : MonoBehaviour
{
	public enum OutlineMode
	{
		MouseOver = 0,
		UserCall = 1,
		AllwaysOn = 2
	}

	public Color outlineColor = Color.red;

	public OutlineMode outlineMode;

	public bool flashing;

	[Range(0.5f, 5f)]
	public float flashSpeed = 1.5f;

	public float threshold = 1f;

	private float timeBlink;

	private float deltaRate;

	private Texture2D maskTexture;

	private Texture2D maskNone;

	private List<Material> materials = new List<Material>();

	private bool ApplyEffect = true;

	private bool currentMode;

	private bool blinkMode;

	private float andThr;

	private Color outAnt;

	private void Awake()
	{
		materials.Clear();
		maskTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
		outlineColor.a = 1f;
		maskTexture.SetPixel(0, 0, outlineColor);
		maskTexture.SetPixel(1, 0, outlineColor);
		maskTexture.SetPixel(0, 1, outlineColor);
		maskTexture.SetPixel(1, 1, outlineColor);
		maskTexture.Apply();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			materials.Add(renderer.material);
		}
		andThr = threshold;
		outAnt = outlineColor;
		maskNone = new Texture2D(2, 2, TextureFormat.ARGB32, false);
		maskNone.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
		maskNone.SetPixel(1, 0, new Color(0f, 0f, 0f, 0f));
		maskNone.SetPixel(0, 1, new Color(0f, 0f, 0f, 0f));
		maskNone.SetPixel(1, 1, new Color(0f, 0f, 0f, 0f));
		maskNone.Apply();
		if (outlineMode == OutlineMode.AllwaysOn)
		{
			ApplyEffect = true;
		}
		else
		{
			ApplyEffect = false;
		}
	}

	private void OnEnable()
	{
		if (ApplyEffect)
		{
			foreach (Material material in materials)
			{
				material.SetTexture("_SpriteMask", maskTexture);
			}
		}
		else
		{
			foreach (Material material2 in materials)
			{
				material2.SetTexture("_SpriteMask", maskNone);
			}
		}
		foreach (Material material3 in materials)
		{
			material3.SetFloat("_Threshold", threshold);
		}
	}

	private void OnMouseEnter()
	{
		if (outlineMode == OutlineMode.MouseOver)
		{
			ApplyEffect = true;
		}
	}

	public void UserCall(bool isOn)
	{
		ApplyEffect = isOn;
	}

	private void OnMouseExit()
	{
		if (outlineMode == OutlineMode.MouseOver)
		{
			ApplyEffect = false;
		}
	}

	private void LightOn()
	{
		if (currentMode)
		{
			return;
		}
		currentMode = true;
		foreach (Material material in materials)
		{
			material.SetTexture("_SpriteMask", maskTexture);
		}
	}

	private void LightOff()
	{
		if (!currentMode)
		{
			return;
		}
		currentMode = false;
		foreach (Material material in materials)
		{
			material.SetTexture("_SpriteMask", maskNone);
		}
	}

	private void Update()
	{
		if (flashing && ApplyEffect)
		{
			deltaRate = 1f / (2f * flashSpeed);
			if (Time.time - timeBlink > deltaRate)
			{
				timeBlink = Time.time;
				blinkMode = !blinkMode;
				if (!blinkMode)
				{
					LightOff();
				}
				else
				{
					LightOn();
				}
			}
		}
		else if (ApplyEffect)
		{
			LightOn();
		}
		else
		{
			LightOff();
		}
		if (andThr != threshold)
		{
			andThr = threshold;
			foreach (Material material in materials)
			{
				material.SetFloat("_Threshold", threshold);
			}
		}
		if (outAnt.Equals(outlineColor))
		{
			return;
		}
		outlineColor.a = 1f;
		outAnt = outlineColor;
		maskTexture.SetPixel(0, 0, outlineColor);
		maskTexture.SetPixel(1, 0, outlineColor);
		maskTexture.SetPixel(0, 1, outlineColor);
		maskTexture.SetPixel(1, 1, outlineColor);
		maskTexture.Apply();
		foreach (Material material2 in materials)
		{
			material2.SetTexture("_SpriteMask", maskTexture);
		}
	}

	private void OnDisable()
	{
		foreach (Material material in materials)
		{
			material.SetTexture("_SpriteMask", null);
		}
	}
}
