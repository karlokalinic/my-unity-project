using System;
using UnityEngine.Events;

namespace AC
{
	[Serializable]
	public class ActionEvent : Action
	{
		public UnityEvent unityEvent;

		public UnityEvent skipEvent;

		public bool ignoreWhenSkipping;

		public ActionEvent()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Call event";
			description = "Calls a given function on a GameObject.";
		}

		public override float Run()
		{
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			return 0f;
		}

		public override void Skip()
		{
			if (!ignoreWhenSkipping)
			{
				Run();
			}
			else if (skipEvent != null)
			{
				skipEvent.Invoke();
			}
		}
	}
}
