using System;
using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(AC_Trigger))]
	public class SetTriggerParameters : SetParametersBase
	{
		private AC_Trigger ownTrigger;

		protected void OnEnable()
		{
			ownTrigger = GetComponent<AC_Trigger>();
			EventManager.OnRunTrigger = (EventManager.Delegate_OnRunTrigger)Delegate.Combine(EventManager.OnRunTrigger, new EventManager.Delegate_OnRunTrigger(OnRunTrigger));
		}

		protected void OnDisable()
		{
			EventManager.OnRunTrigger = (EventManager.Delegate_OnRunTrigger)Delegate.Remove(EventManager.OnRunTrigger, new EventManager.Delegate_OnRunTrigger(OnRunTrigger));
		}

		protected void OnRunTrigger(AC_Trigger trigger, GameObject collidingObject)
		{
			if (trigger == ownTrigger && trigger.source == ActionListSource.AssetFile)
			{
				AssignParameterValues(trigger);
			}
		}
	}
}
