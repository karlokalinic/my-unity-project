using AC;
using UnityEngine;

public class HotspotHighlightConnector : MonoBehaviour
{
	private Hotspot hotspot;

	private Highlight highlight;

	private void OnEnable()
	{
		hotspot = GetComponent<Hotspot>();
		highlight = GetComponent<Highlight>();
	}

	private void OnDisable()
	{
		Connect();
	}

	public void Connect()
	{
		if ((bool)hotspot)
		{
			hotspot.highlight = highlight;
		}
	}

	public void Disconnect()
	{
		if ((bool)hotspot)
		{
			hotspot.highlight = null;
		}
	}
}
