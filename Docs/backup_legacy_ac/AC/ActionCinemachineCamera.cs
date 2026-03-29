using System;
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine;
#define AC_HAS_CINEMACHINE
#elif CINEMACHINE
using Cinemachine;
#define AC_HAS_CINEMACHINE
#endif
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCinemachineCamera : Action
	{
#if AC_HAS_CINEMACHINE
		public CinemachineVirtualCameraBase CM_cam;
#else
		public Component CM_cam;
#endif

		public int Priority;

		public ActionCinemachineCamera()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "CM Priority";
			description = "Changes CM priority";
		}

		public override float Run()
		{
#if AC_HAS_CINEMACHINE
			if ((bool)CM_cam)
			{
				if ((bool)CM_cam.GetComponent<CinemachineVirtualCameraBase>())
				{
					CM_cam.GetComponent<CinemachineVirtualCameraBase>().enabled = true;
					CM_cam.MoveToTopOfPrioritySubqueue();
				}
				CM_cam.Priority = Priority;
			}
#else
			if (CM_cam != null)
			{
				System.Reflection.PropertyInfo priorityProperty = CM_cam.GetType().GetProperty("Priority");
				if (priorityProperty != null && priorityProperty.CanWrite)
				{
					priorityProperty.SetValue(CM_cam, Priority);
				}
			}
#endif
			return 0f;
		}
	}
}
