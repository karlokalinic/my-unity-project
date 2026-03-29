using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Hotspots/Hotspot detector")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_detect_hotspots.html")]
	public class DetectHotspots : MonoBehaviour
	{
		protected Hotspot nearestHotspot;

		protected int selected;

		protected List<Hotspot> hotspots = new List<Hotspot>();

		protected int hotspotLayerInt;

		protected int distantHotspotLayerInt;

		public Hotspot NearestHotspot
		{
			get
			{
				return nearestHotspot;
			}
		}

		protected void Start()
		{
			if (KickStarter.settingsManager != null)
			{
				string text = LayerMask.LayerToName(base.gameObject.layer);
				if (text == KickStarter.settingsManager.hotspotLayer)
				{
					ACDebug.LogWarning("The HotspotDetector's layer, " + text + ", is the same used by Hotspots, and will prevent Hotspots from being properly detected. It should be moved to the Ignore Raycast layer.", base.gameObject);
				}
				hotspotLayerInt = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
				distantHotspotLayerInt = LayerMask.NameToLayer(KickStarter.settingsManager.distantHotspotLayer);
			}
		}

		protected void OnTriggerStay(Collider other)
		{
			Hotspot component = other.GetComponent<Hotspot>();
			if (!(component != null) || !component.PlayerIsWithinBoundary() || !IsLayerCorrect(other.gameObject.layer, true))
			{
				return;
			}
			if (nearestHotspot == null || (base.transform.position - other.transform.position).sqrMagnitude <= (base.transform.position - nearestHotspot.transform.position).sqrMagnitude)
			{
				nearestHotspot = component;
			}
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot == component)
				{
					return;
				}
			}
			hotspots.Add(component);
			hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
		}

		protected void OnTriggerStay2D(Collider2D other)
		{
			Hotspot component = other.GetComponent<Hotspot>();
			if (!(component != null) || !component.PlayerIsWithinBoundary() || !IsLayerCorrect(other.gameObject.layer, true))
			{
				return;
			}
			if (nearestHotspot == null || (base.transform.position - other.transform.position).sqrMagnitude <= (base.transform.position - nearestHotspot.transform.position).sqrMagnitude)
			{
				nearestHotspot = component;
			}
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot == component)
				{
					return;
				}
			}
			hotspots.Add(component);
			hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
		}

		protected void OnTriggerExit(Collider other)
		{
			ForceRemoveHotspot(other.GetComponent<Hotspot>());
		}

		public void _Update()
		{
			if ((bool)nearestHotspot && nearestHotspot.gameObject.layer == LayerMask.NameToLayer(AdvGame.GetReferences().settingsManager.deactivatedLayer))
			{
				nearestHotspot = null;
			}
			if (KickStarter.stateHandler != null && KickStarter.stateHandler.IsInGameplay())
			{
				if (KickStarter.playerInput.InputGetButtonDown("CycleHotspotsLeft"))
				{
					CycleHotspots(false);
				}
				else if (KickStarter.playerInput.InputGetButtonDown("CycleHotspotsRight"))
				{
					CycleHotspots(true);
				}
				else if (KickStarter.playerInput.InputGetAxis("CycleHotspots") > 0.1f)
				{
					CycleHotspots(true);
				}
				else if (KickStarter.playerInput.InputGetAxis("CycleHotspots") < -0.1f)
				{
					CycleHotspots(false);
				}
			}
		}

		public void AfterLoad()
		{
			hotspots.Clear();
			hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
			selected = 0;
		}

		public Hotspot[] GetAllDetectedHotspots()
		{
			return hotspots.ToArray();
		}

		public Hotspot GetSelected()
		{
			if (hotspots.Count > 0)
			{
				if (AdvGame.GetReferences().settingsManager.hotspotsInVicinity == HotspotsInVicinity.NearestOnly)
				{
					if (selected >= 0 && hotspots.Count > selected)
					{
						if (IsLayerCorrect(hotspots[selected].gameObject.layer))
						{
							return nearestHotspot;
						}
						nearestHotspot = null;
						hotspots.Remove(nearestHotspot);
						hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
					}
				}
				else if (AdvGame.GetReferences().settingsManager.hotspotsInVicinity == HotspotsInVicinity.CycleMultiple)
				{
					if (selected >= hotspots.Count)
					{
						selected = hotspots.Count - 1;
					}
					else if (selected < 0)
					{
						selected = 0;
					}
					if (IsLayerCorrect(hotspots[selected].gameObject.layer))
					{
						return hotspots[selected];
					}
					if (nearestHotspot == hotspots[selected])
					{
						nearestHotspot = null;
					}
					hotspots.RemoveAt(selected);
					hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
				}
			}
			return null;
		}

		public void ForceRemoveHotspot(Hotspot _hotspot)
		{
			if (!(_hotspot == null))
			{
				if (nearestHotspot == _hotspot)
				{
					nearestHotspot = null;
				}
				if (IsHotspotInTrigger(_hotspot))
				{
					hotspots.Remove(_hotspot);
					hotspots = KickStarter.eventManager.Call_OnModifyHotspotDetectorCollection(this, hotspots);
				}
				if (_hotspot.highlight != null)
				{
					_hotspot.highlight.HighlightOff();
				}
			}
		}

		public bool IsHotspotInTrigger(Hotspot hotspot)
		{
			if (hotspots.Contains(hotspot))
			{
				return true;
			}
			return false;
		}

		public void HighlightAll()
		{
			foreach (Hotspot hotspot in hotspots)
			{
				if (hotspot.highlight != null)
				{
					hotspot.highlight.HighlightOn();
				}
			}
		}

		protected bool IsLayerCorrect(int layerInt, bool distantToo = false)
		{
			if (distantToo)
			{
				if (layerInt == hotspotLayerInt || layerInt == distantHotspotLayerInt)
				{
					return true;
				}
			}
			else if (layerInt == hotspotLayerInt)
			{
				return true;
			}
			return false;
		}

		protected void OnTriggerExit2D(Collider2D other)
		{
			ForceRemoveHotspot(other.GetComponent<Hotspot>());
		}

		protected void CycleHotspots(bool goRight)
		{
			if (goRight)
			{
				selected++;
			}
			else
			{
				selected--;
			}
			if (selected >= hotspots.Count)
			{
				selected = 0;
			}
			else if (selected < 0)
			{
				selected = hotspots.Count - 1;
			}
		}
	}
}
