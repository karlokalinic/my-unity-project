using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionHotspotEnable : Action
	{
		public int parameterID = -1;

		public int constantID;

		public Hotspot hotspot;

		protected Hotspot runtimeHotspot;

		public bool affectChildren;

		public ChangeType changeType;

		public ActionHotspotEnable()
		{
			isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Enable or disable";
			description = "Turns a Hotspot on or off. To record the state of a Hotspot in save games, be sure to add the RememberHotspot script to the Hotspot in question.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile(parameters, parameterID, constantID, hotspot);
		}

		public override float Run()
		{
			if (runtimeHotspot == null)
			{
				return 0f;
			}
			DoChange(runtimeHotspot);
			if (affectChildren)
			{
				Hotspot[] componentsInChildren = runtimeHotspot.GetComponentsInChildren<Hotspot>();
				Hotspot[] array = componentsInChildren;
				foreach (Hotspot hotspot in array)
				{
					if (hotspot != runtimeHotspot)
					{
						DoChange(hotspot);
					}
				}
			}
			return 0f;
		}

		protected void DoChange(Hotspot _hotspot)
		{
			if (changeType == ChangeType.Enable)
			{
				_hotspot.TurnOn();
			}
			else
			{
				_hotspot.TurnOff();
			}
		}

		public static ActionHotspotEnable CreateNew(Hotspot hotspotToAffect, ChangeType changeToMake)
		{
			ActionHotspotEnable actionHotspotEnable = ScriptableObject.CreateInstance<ActionHotspotEnable>();
			actionHotspotEnable.hotspot = hotspotToAffect;
			actionHotspotEnable.changeType = changeToMake;
			return actionHotspotEnable;
		}
	}
}
