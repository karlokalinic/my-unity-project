using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionHighlight : Action
	{
		public enum WhatToHighlight
		{
			SceneObject = 0,
			InventoryItem = 1
		}

		public int parameterID = -1;

		public int constantID;

		public WhatToHighlight whatToHighlight;

		public HighlightType highlightType;

		public bool isInstant;

		public Highlight highlightObject;

		protected Highlight runtimeHighlightObject;

		public int invID;

		protected int invNumber;

		protected InventoryManager inventoryManager;

		public ActionHighlight()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Highlight";
			description = "Gives a glow effect to any mesh object with the Highlight script component attached to it. Can also be used to make Inventory items glow, making it useful for tutorial sections.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				runtimeHighlightObject = AssignFile(parameters, parameterID, constantID, highlightObject);
			}
			else
			{
				invID = AssignInvItemID(parameters, parameterID, invID);
			}
		}

		public override float Run()
		{
			if (whatToHighlight == WhatToHighlight.SceneObject && runtimeHighlightObject == null)
			{
				return 0f;
			}
			if (whatToHighlight == WhatToHighlight.SceneObject)
			{
				if (highlightType == HighlightType.Enable)
				{
					if (isInstant)
					{
						runtimeHighlightObject.HighlightOnInstant();
					}
					else
					{
						runtimeHighlightObject.HighlightOn();
					}
				}
				else if (highlightType == HighlightType.Disable)
				{
					if (isInstant)
					{
						runtimeHighlightObject.HighlightOffInstant();
					}
					else
					{
						runtimeHighlightObject.HighlightOff();
					}
				}
				else if (highlightType == HighlightType.PulseOnce)
				{
					runtimeHighlightObject.Flash();
				}
				else if (highlightType == HighlightType.PulseContinually)
				{
					runtimeHighlightObject.Pulse();
				}
			}
			else if ((bool)KickStarter.runtimeInventory)
			{
				if (highlightType == HighlightType.Enable && isInstant)
				{
					KickStarter.runtimeInventory.HighlightItemOnInstant(invID);
					return 0f;
				}
				if (highlightType == HighlightType.Disable && isInstant)
				{
					KickStarter.runtimeInventory.HighlightItemOffInstant();
					return 0f;
				}
				KickStarter.runtimeInventory.HighlightItem(invID, highlightType);
			}
			return 0f;
		}

		public static ActionHighlight CreateNew_SceneObject(Highlight objectToAffect, HighlightType highlightType, bool isInstant = false)
		{
			ActionHighlight actionHighlight = ScriptableObject.CreateInstance<ActionHighlight>();
			actionHighlight.whatToHighlight = WhatToHighlight.SceneObject;
			actionHighlight.highlightObject = objectToAffect;
			actionHighlight.highlightType = highlightType;
			actionHighlight.isInstant = isInstant;
			return actionHighlight;
		}

		public static ActionHighlight CreateNew_InventoryItem(int itemIDToAffect, HighlightType highlightType, bool isInstant = false)
		{
			ActionHighlight actionHighlight = ScriptableObject.CreateInstance<ActionHighlight>();
			actionHighlight.whatToHighlight = WhatToHighlight.InventoryItem;
			actionHighlight.invID = itemIDToAffect;
			actionHighlight.highlightType = highlightType;
			actionHighlight.isInstant = isInstant;
			return actionHighlight;
		}
	}
}
