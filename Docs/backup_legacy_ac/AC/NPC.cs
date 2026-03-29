using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Characters/NPC")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_n_p_c.html")]
	public class NPC : Char
	{
		public bool moveOutOfPlayersWay;

		protected bool isEvadingPlayer;

		public float minPlayerDistance = 1f;

		protected Char followTarget;

		protected bool followTargetIsPlayer;

		protected float followFrequency;

		protected float followUpdateTimer;

		protected float followDistance;

		protected float followDistanceMax;

		protected bool followFaceWhenIdle;

		protected bool followRandomDirection;

		protected LayerMask LayerOn;

		protected LayerMask LayerOff;

		protected void Awake()
		{
			if (KickStarter.settingsManager != null)
			{
				LayerOn = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
				LayerOff = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
			_Awake();
		}

		public override void _Update()
		{
			if (moveOutOfPlayersWay && charState == CharState.Idle)
			{
				isEvadingPlayer = false;
				if (!activePath || pausePath)
				{
					StayAwayFromPlayer();
				}
			}
			if (followTarget != null)
			{
				followUpdateTimer -= Time.deltaTime;
				if (followUpdateTimer <= 0f)
				{
					FollowUpdate();
				}
			}
			if ((bool)activePath && (bool)followTarget)
			{
				FollowCheckDistance();
				FollowCheckDistanceMax();
			}
			if ((bool)activePath && !pausePath)
			{
				if (IsTurningBeforeWalking())
				{
					if (charState == CharState.Move)
					{
						StartDecelerating();
					}
					else if (charState == CharState.Custom)
					{
						charState = CharState.Idle;
					}
				}
				else
				{
					charState = CharState.Move;
				}
			}
			base._Update();
		}

		protected void StayAwayFromPlayer()
		{
			if (!(KickStarter.player != null) || !(Vector3.Distance(base.transform.position, KickStarter.player.transform.position) < minPlayerDistance))
			{
				return;
			}
			Vector3[] array = TryNavPoint(base.transform.position - KickStarter.player.transform.position);
			int num = 0;
			if (array == null)
			{
				array = TryNavPoint(Vector3.Cross(base.transform.up, base.transform.position - KickStarter.player.transform.position).normalized);
				num++;
			}
			if (array == null)
			{
				array = TryNavPoint(Vector3.Cross(-base.transform.up, base.transform.position - KickStarter.player.transform.position).normalized);
				num++;
			}
			if (array == null)
			{
				array = TryNavPoint(KickStarter.player.transform.position - base.transform.position);
				num++;
			}
			if (array != null)
			{
				if (num == 0)
				{
					MoveAlongPoints(array, false);
				}
				else
				{
					MoveToPoint(array[array.Length - 1]);
				}
				isEvadingPlayer = true;
				followUpdateTimer = followFrequency;
			}
		}

		protected Vector3[] TryNavPoint(Vector3 _direction)
		{
			float magnitude = _direction.magnitude;
			Vector3 vector = base.transform.position + _direction.normalized * (minPlayerDistance - magnitude) * 1.2f;
			if (SceneSettings.ActInScreenSpace())
			{
				vector = AdvGame.GetScreenNavMesh(vector);
			}
			else if (SceneSettings.CameraPerspective == CameraPerspective.ThreeD)
			{
				vector.y = base.transform.position.y;
			}
			Vector3[] pointsArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(base.transform.position, vector, this);
			if (pointsArray.Length == 0 || Vector3.Distance(pointsArray[pointsArray.Length - 1], base.transform.position) < minPlayerDistance * 0.6f)
			{
				return null;
			}
			return pointsArray;
		}

		public void StopFollowing()
		{
			FollowStop();
			followTarget = null;
			followTargetIsPlayer = false;
			followFrequency = 0f;
			followDistance = 0f;
		}

		protected void FollowUpdate()
		{
			followUpdateTimer = followFrequency;
			float num = FollowCheckDistance();
			if (!(num > followDistance))
			{
				return;
			}
			Paths component = GetComponent<Paths>();
			if (component == null)
			{
				ACDebug.LogWarning("Cannot move a character with no Paths component", base.gameObject);
				return;
			}
			component.pathType = AC_PathType.ForwardOnly;
			component.affectY = true;
			Vector3 vector = followTarget.transform.position;
			if (SceneSettings.ActInScreenSpace())
			{
				vector = AdvGame.GetScreenNavMesh(vector);
			}
			Vector3[] pointData;
			if ((bool)KickStarter.navigationManager)
			{
				if (followRandomDirection)
				{
					vector = KickStarter.navigationManager.navigationEngine.GetPointNear(vector, followDistance, followDistanceMax);
				}
				pointData = KickStarter.navigationManager.navigationEngine.GetPointsArray(base.transform.position, vector, this);
			}
			else
			{
				List<Vector3> list = new List<Vector3>();
				list.Add(vector);
				pointData = list.ToArray();
			}
			if (num > followDistanceMax)
			{
				MoveAlongPoints(pointData, true);
			}
			else
			{
				MoveAlongPoints(pointData, false);
			}
			isEvadingPlayer = false;
		}

		protected float FollowCheckDistance()
		{
			float num = Vector3.Distance(followTarget.transform.position, base.transform.position);
			if (num < followDistance && !isEvadingPlayer)
			{
				if (!followRandomDirection)
				{
					EndPath();
				}
				if (activePath == null && followFaceWhenIdle)
				{
					Vector3 direction = followTarget.transform.position - base.transform.position;
					SetLookDirection(direction, false);
				}
			}
			return num;
		}

		protected void FollowCheckDistanceMax()
		{
			if (!followTarget)
			{
				return;
			}
			if (FollowCheckDistance() > followDistanceMax)
			{
				if (!base.isRunning)
				{
					FollowUpdate();
				}
			}
			else if (base.isRunning)
			{
				FollowUpdate();
			}
		}

		protected void FollowStop()
		{
			if (followTarget != null)
			{
				EndPath();
			}
		}

		public void FollowAssign(Char _followTarget, bool _followTargetIsPlayer, float _followFrequency, float _followDistance, float _followDistanceMax, bool _faceWhenIdle, bool _followRandomDirection)
		{
			if (_followTargetIsPlayer)
			{
				_followTarget = KickStarter.player;
			}
			if (_followTarget == null || _followFrequency <= 0f || _followDistance <= 0f || _followDistanceMax <= 0f)
			{
				StopFollowing();
				if (_followTarget == null)
				{
					ACDebug.LogWarning("NPC " + base.name + " cannot follow because no target was set.", this);
				}
				else if (_followFrequency <= 0f)
				{
					ACDebug.LogWarning("NPC " + base.name + " cannot follow because frequency was zero.", this);
				}
				else if (_followDistance <= 0f)
				{
					ACDebug.LogWarning("NPC " + base.name + " cannot follow because distance was zero.", this);
				}
				else if (_followDistanceMax <= 0f)
				{
					ACDebug.LogWarning("NPC " + base.name + " cannot follow because max distance was zero.", this);
				}
			}
			else
			{
				followTarget = _followTarget;
				followTargetIsPlayer = _followTargetIsPlayer;
				followFrequency = _followFrequency;
				followUpdateTimer = followFrequency;
				followDistance = _followDistance;
				followDistanceMax = _followDistanceMax;
				followFaceWhenIdle = _faceWhenIdle;
				followRandomDirection = _followRandomDirection;
			}
		}

		protected void TurnOn()
		{
			base.gameObject.layer = LayerOn;
		}

		protected void TurnOff()
		{
			base.gameObject.layer = LayerOff;
		}

		public NPCData SaveData(NPCData npcData)
		{
			npcData.RotX = base.TransformRotation.eulerAngles.x;
			npcData.RotY = base.TransformRotation.eulerAngles.y;
			npcData.RotZ = base.TransformRotation.eulerAngles.z;
			npcData.inCustomCharState = charState == CharState.Custom && GetAnimator() != null && (bool)GetAnimator().GetComponent<RememberAnimator>();
			npcData = GetAnimEngine().SaveNPCData(npcData, this);
			npcData.walkSound = AssetLoader.GetAssetInstanceID(walkSound);
			npcData.runSound = AssetLoader.GetAssetInstanceID(runSound);
			npcData.speechLabel = GetName();
			npcData.displayLineID = displayLineID;
			npcData.portraitGraphic = AssetLoader.GetAssetInstanceID(portraitIcon.texture);
			npcData.walkSpeed = walkSpeedScale;
			npcData.runSpeed = runSpeedScale;
			npcData.lockDirection = lockDirection;
			npcData.lockScale = lockScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<FollowSortingMap>())
			{
				npcData.lockSorting = spriteChild.GetComponent<FollowSortingMap>().lockSorting;
			}
			else if ((bool)GetComponent<FollowSortingMap>())
			{
				npcData.lockSorting = GetComponent<FollowSortingMap>().lockSorting;
			}
			else
			{
				npcData.lockSorting = false;
			}
			npcData.spriteDirection = spriteDirection;
			npcData.spriteScale = spriteScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<Renderer>())
			{
				npcData.sortingOrder = spriteChild.GetComponent<Renderer>().sortingOrder;
				npcData.sortingLayer = spriteChild.GetComponent<Renderer>().sortingLayerName;
			}
			else if ((bool)GetComponent<Renderer>())
			{
				npcData.sortingOrder = GetComponent<Renderer>().sortingOrder;
				npcData.sortingLayer = GetComponent<Renderer>().sortingLayerName;
			}
			npcData.pathID = 0;
			npcData.lastPathID = 0;
			if ((bool)GetPath())
			{
				npcData.targetNode = GetTargetNode();
				npcData.prevNode = GetPreviousNode();
				npcData.isRunning = base.isRunning;
				npcData.pathAffectY = GetPath().affectY;
				if (GetPath() == GetComponent<Paths>())
				{
					npcData.pathData = Serializer.CreatePathData(GetComponent<Paths>());
				}
				else if ((bool)GetPath().GetComponent<ConstantID>())
				{
					npcData.pathID = GetPath().GetComponent<ConstantID>().constantID;
				}
				else
				{
					ACDebug.LogWarning("Want to save path data for " + base.name + " but path has no ID!", base.gameObject);
				}
			}
			if ((bool)GetLastPath())
			{
				npcData.lastTargetNode = GetLastTargetNode();
				npcData.lastPrevNode = GetLastPrevNode();
				if ((bool)GetLastPath().GetComponent<ConstantID>())
				{
					npcData.lastPathID = GetLastPath().GetComponent<ConstantID>().constantID;
				}
				else
				{
					ACDebug.LogWarning("Want to save previous path data for " + base.name + " but path has no ID!", base.gameObject);
				}
			}
			if ((bool)followTarget)
			{
				if (!followTargetIsPlayer)
				{
					if ((bool)followTarget.GetComponent<ConstantID>())
					{
						npcData.followTargetID = followTarget.GetComponent<ConstantID>().constantID;
						npcData.followTargetIsPlayer = followTargetIsPlayer;
						npcData.followFrequency = followFrequency;
						npcData.followDistance = followDistance;
						npcData.followDistanceMax = followDistanceMax;
						npcData.followFaceWhenIdle = followFaceWhenIdle;
						npcData.followRandomDirection = followRandomDirection;
					}
					else
					{
						ACDebug.LogWarning("Want to save follow data for " + base.name + " but " + followTarget.name + " has no ID!", base.gameObject);
					}
				}
				else
				{
					npcData.followTargetID = 0;
					npcData.followTargetIsPlayer = followTargetIsPlayer;
					npcData.followFrequency = followFrequency;
					npcData.followDistance = followDistance;
					npcData.followDistanceMax = followDistanceMax;
					npcData.followFaceWhenIdle = followFaceWhenIdle;
					npcData.followRandomDirection = followRandomDirection;
				}
			}
			else
			{
				npcData.followTargetID = 0;
				npcData.followTargetIsPlayer = false;
				npcData.followFrequency = 0f;
				npcData.followDistance = 0f;
				npcData.followDistanceMax = 0f;
				npcData.followFaceWhenIdle = false;
				npcData.followRandomDirection = false;
			}
			if (headFacing == HeadFacing.Manual && headTurnTarget != null)
			{
				npcData.isHeadTurning = true;
				npcData.headTargetID = Serializer.GetConstantID(headTurnTarget);
				if (npcData.headTargetID == 0)
				{
					ACDebug.LogWarning(string.Concat("The NPC ", base.gameObject.name, "'s head-turning target Transform, ", headTurnTarget, ", was not saved because it has no Constant ID"), base.gameObject);
				}
				npcData.headTargetX = headTurnTargetOffset.x;
				npcData.headTargetY = headTurnTargetOffset.y;
				npcData.headTargetZ = headTurnTargetOffset.z;
			}
			else
			{
				npcData.isHeadTurning = false;
				npcData.headTargetID = 0;
				npcData.headTargetX = 0f;
				npcData.headTargetY = 0f;
				npcData.headTargetZ = 0f;
			}
			if (GetComponentInChildren<FollowSortingMap>() != null)
			{
				FollowSortingMap componentInChildren = GetComponentInChildren<FollowSortingMap>();
				npcData.followSortingMap = componentInChildren.followSortingMap;
				if (!npcData.followSortingMap && componentInChildren.GetSortingMap() != null)
				{
					if (componentInChildren.GetSortingMap().GetComponent<ConstantID>() != null)
					{
						npcData.customSortingMapID = componentInChildren.GetSortingMap().GetComponent<ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning("The NPC " + base.gameObject.name + "'s SortingMap, " + componentInChildren.GetSortingMap().name + ", was not saved because it has no Constant ID");
						npcData.customSortingMapID = 0;
					}
				}
				else
				{
					npcData.customSortingMapID = 0;
				}
			}
			else
			{
				npcData.followSortingMap = false;
				npcData.customSortingMapID = 0;
			}
			return npcData;
		}

		public void LoadData(NPCData data)
		{
			charState = (data.inCustomCharState ? CharState.Custom : CharState.Idle);
			EndPath();
			GetAnimEngine().LoadNPCData(data, this);
			walkSound = AssetLoader.RetrieveAsset(walkSound, data.walkSound);
			runSound = AssetLoader.RetrieveAsset(runSound, data.runSound);
			if (!string.IsNullOrEmpty(data.speechLabel))
			{
				SetName(data.speechLabel, data.displayLineID);
			}
			portraitIcon.ReplaceTexture(AssetLoader.RetrieveAsset(portraitIcon.texture, data.portraitGraphic));
			walkSpeedScale = data.walkSpeed;
			runSpeedScale = data.runSpeed;
			lockDirection = data.lockDirection;
			lockScale = data.lockScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<FollowSortingMap>())
			{
				spriteChild.GetComponent<FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else if ((bool)GetComponent<FollowSortingMap>())
			{
				GetComponent<FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else
			{
				ReleaseSorting();
			}
			if (data.lockDirection)
			{
				spriteDirection = data.spriteDirection;
			}
			if (data.lockScale)
			{
				spriteScale = data.spriteScale;
			}
			if (data.lockSorting)
			{
				if ((bool)spriteChild && (bool)spriteChild.GetComponent<Renderer>())
				{
					spriteChild.GetComponent<Renderer>().sortingOrder = data.sortingOrder;
					spriteChild.GetComponent<Renderer>().sortingLayerName = data.sortingLayer;
				}
				else if ((bool)GetComponent<Renderer>())
				{
					GetComponent<Renderer>().sortingOrder = data.sortingOrder;
					GetComponent<Renderer>().sortingLayerName = data.sortingLayer;
				}
			}
			Char obj = null;
			if (data.followTargetID != 0)
			{
				RememberNPC rememberNPC = Serializer.returnComponent<RememberNPC>(data.followTargetID);
				if ((bool)rememberNPC.GetComponent<Char>())
				{
					obj = rememberNPC.GetComponent<Char>();
				}
			}
			if (obj != null || (data.followTargetIsPlayer && KickStarter.player != null))
			{
				FollowAssign(obj, data.followTargetIsPlayer, data.followFrequency, data.followDistance, data.followDistanceMax, data.followFaceWhenIdle, data.followRandomDirection);
			}
			else
			{
				StopFollowing();
			}
			Halt();
			if (!string.IsNullOrEmpty(data.pathData) && (bool)GetComponent<Paths>())
			{
				Paths component = GetComponent<Paths>();
				component = Serializer.RestorePathData(component, data.pathData);
				SetPath(component, data.targetNode, data.prevNode, data.pathAffectY);
				base.isRunning = data.isRunning;
			}
			else if (data.pathID != 0)
			{
				Paths paths = Serializer.returnComponent<Paths>(data.pathID);
				if (paths != null)
				{
					SetPath(paths, data.targetNode, data.prevNode);
				}
				else
				{
					ACDebug.LogWarning("Trying to assign a path for NPC " + base.name + ", but the path was not found - was it deleted?", base.gameObject);
				}
			}
			if (data.lastPathID != 0)
			{
				Paths paths2 = Serializer.returnComponent<Paths>(data.lastPathID);
				if (paths2 != null)
				{
					SetLastPath(paths2, data.lastTargetNode, data.lastPrevNode);
				}
				else
				{
					ACDebug.LogWarning("Trying to assign the previous path for NPC " + base.name + ", but the path was not found - was it deleted?", base.gameObject);
				}
			}
			if (data.isHeadTurning)
			{
				ConstantID constantID = Serializer.returnComponent<ConstantID>(data.headTargetID);
				if (constantID != null)
				{
					SetHeadTurnTarget(constantID.transform, new Vector3(data.headTargetX, data.headTargetY, data.headTargetZ), true);
				}
				else
				{
					ClearHeadTurnTarget(true);
				}
			}
			else
			{
				ClearHeadTurnTarget(true);
			}
			if (GetComponentsInChildren<FollowSortingMap>() == null)
			{
				return;
			}
			FollowSortingMap[] componentsInChildren = GetComponentsInChildren<FollowSortingMap>();
			SortingMap sortingMap = Serializer.returnComponent<SortingMap>(data.customSortingMapID);
			FollowSortingMap[] array = componentsInChildren;
			foreach (FollowSortingMap followSortingMap in array)
			{
				followSortingMap.followSortingMap = data.followSortingMap;
				if (!data.followSortingMap && sortingMap != null)
				{
					followSortingMap.SetSortingMap(sortingMap);
				}
				else
				{
					followSortingMap.SetSortingMap(KickStarter.sceneSettings.sortingMap);
				}
			}
		}

		public Char GetFollowTarget()
		{
			return followTarget;
		}

		public void HideFromView(Player player = null)
		{
			Halt();
			Teleport(base.transform.position + new Vector3(100f, -100f, 100f));
			if (player != null)
			{
				ACDebug.Log("NPC '" + GetName() + "' was moved out of the way to make way for the associated Player '" + player.GetName() + "'.", this);
			}
		}
	}
}
