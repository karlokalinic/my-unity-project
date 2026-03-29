using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class PlayerData
	{
		public int playerID;

		public int currentScene;

		public int previousScene;

		public string currentSceneName;

		public string previousSceneName;

		public string openSubScenes;

		public float playerLocX;

		public float playerLocY;

		public float playerLocZ;

		public float playerRotY;

		public float playerWalkSpeed;

		public float playerRunSpeed;

		public string playerIdleAnim;

		public string playerWalkAnim;

		public string playerTalkAnim;

		public string playerRunAnim;

		public string playerWalkSound;

		public string playerRunSound;

		public string playerPortraitGraphic;

		public string playerSpeechLabel;

		public int playerDisplayLineID;

		public int playerTargetNode;

		public int playerPrevNode;

		public string playerPathData;

		public bool playerIsRunning;

		public bool playerLockedPath;

		public int playerActivePath;

		public bool playerPathAffectY;

		public int lastPlayerTargetNode;

		public int lastPlayerPrevNode;

		public int lastPlayerActivePath;

		public bool playerUpLock;

		public bool playerDownLock;

		public bool playerLeftlock;

		public bool playerRightLock;

		public int playerRunLock;

		public bool playerFreeAimLock;

		public bool playerIgnoreGravity;

		public bool playerLockDirection;

		public string playerSpriteDirection;

		public bool playerLockScale;

		public float playerSpriteScale;

		public bool playerLockSorting;

		public int playerSortingOrder;

		public string playerSortingLayer;

		public string inventoryData;

		public bool inCustomCharState;

		public bool playerLockHotspotHeadTurning;

		public bool isHeadTurning;

		public int headTargetID;

		public float headTargetX;

		public float headTargetY;

		public float headTargetZ;

		public int gameCamera;

		public int lastNavCamera;

		public int lastNavCamera2;

		public float mainCameraLocX;

		public float mainCameraLocY;

		public float mainCameraLocZ;

		public float mainCameraRotX;

		public float mainCameraRotY;

		public float mainCameraRotZ;

		public bool isSplitScreen;

		public bool isTopLeftSplit;

		public bool splitIsVertical;

		public int splitCameraID;

		public float splitAmountMain;

		public float splitAmountOther;

		public float shakeIntensity;

		public float shakeDuration;

		public int shakeEffect;

		public float overlayRectX;

		public float overlayRectY;

		public float overlayRectWidth;

		public float overlayRectHeight;

		public bool followSortingMap;

		public int customSortingMapID;

		public int activeDocumentID = -1;

		public string collectedDocumentData;

		public string lastOpenDocumentPagesData;

		public string playerObjectivesData;

		public List<ScriptData> playerScriptData;

		public void UpdatePosition(SceneInfo newSceneInfo, Vector3 newPosition, Quaternion newRotation, _Camera associatedCamera)
		{
			currentScene = newSceneInfo.number;
			currentSceneName = newSceneInfo.name;
			playerLocX = newPosition.x;
			playerLocY = newPosition.y;
			playerLocZ = newPosition.z;
			playerRotY = newRotation.eulerAngles.y;
			if (associatedCamera != null)
			{
				gameCamera = 0;
				ConstantID component = associatedCamera.GetComponent<ConstantID>();
				if (component != null)
				{
					gameCamera = component.constantID;
				}
				else
				{
					Debug.LogWarning(string.Concat("Cannot save Player's active camera because ", associatedCamera, " has no ConstantID component"), associatedCamera);
				}
			}
		}
	}
}
