using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCameraSplit : Action
	{
		public int parameterID1 = -1;

		public int parameterID2 = -1;

		public int constantID1;

		public int constantID2;

		public float splitAmount1 = 0.49f;

		public float splitAmount2 = 0.49f;

		public _Camera cam1;

		public _Camera cam2;

		protected _Camera runtimeCam1;

		protected _Camera runtimeCam2;

		public Rect overlayRect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

		public bool turnOff;

		public CameraSplitOrientation orientation;

		public bool mainIsTopLeft;

		public ActionCameraSplit()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Split-screen";
			description = "Displays two cameras on the screen at once, arranged either horizontally or vertically. Which camera is the 'main' (i.e. which one responds to mouse clicks) can also be set.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeCam1 = AssignFile(parameters, parameterID1, constantID1, cam1);
			runtimeCam2 = AssignFile(parameters, parameterID2, constantID2, cam2);
		}

		public override float Run()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			mainCamera.RemoveSplitScreen();
			if (turnOff || runtimeCam1 == null || runtimeCam2 == null)
			{
				return 0f;
			}
			if (orientation == CameraSplitOrientation.Overlay)
			{
				mainCamera.SetBoxOverlay(runtimeCam1, runtimeCam2, overlayRect);
			}
			else
			{
				if (splitAmount1 + splitAmount2 > 1f)
				{
					splitAmount2 = 1f - splitAmount1;
				}
				if (mainIsTopLeft)
				{
					mainCamera.SetSplitScreen(runtimeCam1, runtimeCam2, orientation, mainIsTopLeft, splitAmount1, splitAmount2);
				}
				else
				{
					mainCamera.SetSplitScreen(runtimeCam2, runtimeCam1, orientation, mainIsTopLeft, splitAmount1, splitAmount2);
				}
			}
			return 0f;
		}

		public static ActionCameraSplit CreateNew_Overlay(_Camera underlayCamera, _Camera overlayCamera, Rect overlayRect)
		{
			ActionCameraSplit actionCameraSplit = ScriptableObject.CreateInstance<ActionCameraSplit>();
			actionCameraSplit.orientation = CameraSplitOrientation.Overlay;
			actionCameraSplit.cam1 = underlayCamera;
			actionCameraSplit.cam2 = overlayCamera;
			actionCameraSplit.overlayRect = overlayRect;
			return actionCameraSplit;
		}

		public static ActionCameraSplit CreateNew_AboveAndBelow(_Camera topCamera, _Camera bottomCamera, bool topIsActive = true, float topCameraSpace = 0.49f, float bottomCameraSpace = 0.49f)
		{
			ActionCameraSplit actionCameraSplit = ScriptableObject.CreateInstance<ActionCameraSplit>();
			actionCameraSplit.orientation = CameraSplitOrientation.Horizontal;
			actionCameraSplit.cam1 = topCamera;
			actionCameraSplit.cam2 = bottomCamera;
			actionCameraSplit.mainIsTopLeft = topIsActive;
			actionCameraSplit.splitAmount1 = topCameraSpace;
			actionCameraSplit.splitAmount2 = bottomCameraSpace;
			return actionCameraSplit;
		}

		public static ActionCameraSplit CreateNew_SideBySide(_Camera leftCamera, _Camera rightCamera, bool leftIsActive = true, float leftCameraSpace = 0.49f, float rightCameraSpace = 0.49f)
		{
			ActionCameraSplit actionCameraSplit = ScriptableObject.CreateInstance<ActionCameraSplit>();
			actionCameraSplit.orientation = CameraSplitOrientation.Vertical;
			actionCameraSplit.cam1 = leftCamera;
			actionCameraSplit.cam2 = rightCamera;
			actionCameraSplit.mainIsTopLeft = leftIsActive;
			actionCameraSplit.splitAmount1 = leftCameraSpace;
			actionCameraSplit.splitAmount2 = rightCameraSpace;
			return actionCameraSplit;
		}
	}
}
