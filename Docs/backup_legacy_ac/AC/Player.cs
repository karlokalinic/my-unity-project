using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Characters/Player")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player.html")]
	public class Player : Char
	{
		public AnimationClip jumpAnim;

		public int ID;

		public DetectHotspots hotspotDetector;

		public NPC associatedNPCPrefab;

		protected bool lockedPath;

		protected float tankTurnFloat;

		public bool toggleRun;

		protected bool lockHotspotHeadTurning;

		protected Transform fpCam;

		protected FirstPersonCamera firstPersonCamera;

		protected bool prepareToJump;

		protected SkinnedMeshRenderer[] skinnedMeshRenderers;

		public Transform FirstPersonCamera
		{
			get
			{
				return fpCam;
			}
			set
			{
				fpCam = value;
			}
		}

		protected void Awake()
		{
			if ((bool)soundChild && (bool)soundChild.gameObject.GetComponent<AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent<AudioSource>();
			}
			skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			if ((bool)KickStarter.playerMovement)
			{
				Transform transform = KickStarter.playerMovement.AssignFPCamera();
				if (transform != null)
				{
					fpCam = KickStarter.playerMovement.AssignFPCamera();
					if (fpCam != null)
					{
						firstPersonCamera = fpCam.GetComponent<FirstPersonCamera>();
					}
				}
			}
			_Awake();
			if (GetAnimEngine() != null && KickStarter.settingsManager != null && KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && GetAnimEngine().isSpriteBased && hotspotDetector != null && (!(spriteChild != null) || !(hotspotDetector.transform == spriteChild)) && turn2DCharactersIn3DSpace)
			{
				if (hotspotDetector.transform == base.transform)
				{
					ACDebug.LogWarning("The Player '" + base.name + "' has a Hotspot Detector assigned, but it is on the root.  Either parent it to the 'sprite child' instead, or check 'Turn root object in 3D space?' in the Player Inspector.", this);
				}
				else if (hotspotDetector.transform.parent == base.transform)
				{
					ACDebug.LogWarning("The Player '" + base.name + "' has a Hotspot Detector assigned, but it is a direct child of a 2D root.  Either parent it to the 'sprite child' instead, or check 'Turn root object in 3D space?' in the Player Inspector.", this);
				}
			}
		}

		public void Initialise()
		{
			if ((bool)GetAnimation())
			{
				AdvGame.PlayAnimClip(GetAnimation(), AdvGame.GetAnimLayerInt(AnimLayer.Base), idleAnim, AnimationBlendMode.Blend, WrapMode.Loop);
			}
			else if ((bool)spriteChild)
			{
				InitSpriteChild();
			}
			UpdateScale();
			GetAnimEngine().TurnHead(Vector2.zero);
			GetAnimEngine().PlayIdle();
		}

		public override void _Update()
		{
			bool flag = false;
			if (KickStarter.playerInput.InputGetButtonDown("Jump") && KickStarter.stateHandler.IsInGameplay() && motionControl == MotionControl.Automatic && !KickStarter.stateHandler.MovementIsOff && !KickStarter.playerInput.IsJumpLocked)
			{
				flag = Jump();
			}
			if ((bool)hotspotDetector)
			{
				hotspotDetector._Update();
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
				else if ((KickStarter.stateHandler.gameState == GameState.Cutscene && !lockedPath) || KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick || KickStarter.settingsManager.movementMethod == MovementMethod.None || (KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor && KickStarter.settingsManager.singleTapStraight) || IsMovingToHotspot())
				{
					charState = CharState.Move;
				}
			}
			else if (activePath == null && charState == CharState.Move && !KickStarter.stateHandler.IsInGameplay() && KickStarter.stateHandler.gameState != GameState.Paused)
			{
				StartDecelerating();
			}
			if (isJumping && !flag && IsGrounded())
			{
				isJumping = false;
			}
			base._Update();
		}

		public void TankTurnLeft(float intensity = 1f)
		{
			lookDirection = -(intensity * base.TransformRight) + (1f - intensity) * base.TransformForward;
			tankTurning = true;
			turnFloat = (tankTurnFloat = 0f - intensity);
		}

		public void TankTurnRight(float intensity = 1f)
		{
			lookDirection = intensity * base.TransformRight + (1f - intensity) * base.TransformForward;
			tankTurning = true;
			turnFloat = (tankTurnFloat = intensity);
		}

		public void CancelPathfindRecalculations()
		{
			pathfindUpdateTime = 0f;
		}

		public override void StopTankTurning()
		{
			lookDirection = base.TransformForward;
			tankTurning = false;
		}

		public override float GetTurnFloat()
		{
			if (tankTurning)
			{
				return tankTurnFloat;
			}
			return base.GetTurnFloat();
		}

		public void ForceTurnFloat(float _value)
		{
			turnFloat = _value;
		}

		public bool Jump()
		{
			if (isJumping)
			{
				return false;
			}
			if (IsGrounded() && activePath == null)
			{
				if (_rigidbody != null && !_rigidbody.isKinematic)
				{
					if (useRigidbodyForMovement)
					{
						prepareToJump = true;
					}
					else
					{
						_rigidbody.linearVelocity = Vector3.up * KickStarter.settingsManager.jumpSpeed;
					}
					isJumping = true;
					if (ignoreGravity)
					{
						ACDebug.LogWarning(base.gameObject.name + " is jumping - but 'Ignore gravity?' is enabled in the Player Inspector. Is this correct?", base.gameObject);
					}
					return true;
				}
				if (motionControl == MotionControl.Automatic)
				{
					if (_rigidbody != null && _rigidbody.isKinematic)
					{
						ACDebug.Log("Player cannot jump without a non-kinematic Rigidbody component.", base.gameObject);
					}
					else
					{
						ACDebug.Log("Player cannot jump without a Rigidbody component.", base.gameObject);
					}
				}
			}
			else if (_collider == null)
			{
				ACDebug.Log(base.gameObject.name + " has no Collider component", base.gameObject);
			}
			return false;
		}

		public override void _FixedUpdate()
		{
			if (prepareToJump)
			{
				prepareToJump = false;
				_rigidbody.AddForce(Vector3.up * KickStarter.settingsManager.jumpSpeed, ForceMode.Impulse);
			}
			base._FixedUpdate();
		}

		protected bool IsMovingToHotspot()
		{
			if (KickStarter.playerInteraction != null && KickStarter.playerInteraction.GetHotspotMovingTo() != null)
			{
				return true;
			}
			return false;
		}

		public new void EndPath()
		{
			lockedPath = false;
			base.EndPath();
		}

		public void SetLockedPath(Paths pathOb)
		{
			if (!KickStarter.settingsManager)
			{
				return;
			}
			if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct)
			{
				lockedPath = true;
				if (pathOb.pathSpeed == PathSpeed.Run)
				{
					base.isRunning = true;
				}
				else
				{
					base.isRunning = false;
				}
				if (pathOb.affectY)
				{
					base.transform.position = pathOb.transform.position;
				}
				else
				{
					base.transform.position = new Vector3(pathOb.transform.position.x, base.transform.position.y, pathOb.transform.position.z);
				}
				activePath = pathOb;
				targetNode = 1;
				charState = CharState.Idle;
			}
			else
			{
				ACDebug.LogWarning("Path-constrained player movement is only available with Direct control.", base.gameObject);
			}
		}

		public bool IsLockedToPath()
		{
			return lockedPath;
		}

		public override bool CanBeDirectControlled()
		{
			if (KickStarter.stateHandler.gameState == GameState.Normal && (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson))
			{
				return KickStarter.playerInput.CanDirectControlPlayer();
			}
			return false;
		}

		protected override void Accelerate()
		{
			float num = GetTargetSpeed();
			if (AccurateDestination() && WillStopAtNextNode())
			{
				AccurateAcc(GetTargetSpeed(), false);
				return;
			}
			if (KickStarter.settingsManager.magnitudeAffectsDirect && KickStarter.settingsManager.movementMethod == MovementMethod.Direct && KickStarter.stateHandler.IsInGameplay() && !IsMovingToHotspot())
			{
				num -= (1f - KickStarter.playerInput.GetMoveKeys().magnitude) / 2f;
			}
			moveSpeed = moveSpeedLerp.Update(moveSpeed, num, acceleration);
		}

		public bool IsTilting()
		{
			if (firstPersonCamera != null)
			{
				return firstPersonCamera.IsTilting();
			}
			return false;
		}

		public float GetTilt()
		{
			if (firstPersonCamera != null)
			{
				return firstPersonCamera.GetTilt();
			}
			return 0f;
		}

		public void SetTilt(Vector3 lookAtPosition, bool isInstant)
		{
			if (fpCam == null)
			{
				return;
			}
			if (isInstant)
			{
				float num = Mathf.Asin((lookAtPosition - fpCam.position).normalized.y) * 57.29578f;
				firstPersonCamera.SetPitch(0f - num);
				return;
			}
			Quaternion rotation = fpCam.rotation;
			fpCam.transform.LookAt(lookAtPosition);
			float num2 = fpCam.localEulerAngles.x;
			fpCam.rotation = rotation;
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			firstPersonCamera.SetPitch(num2, false);
		}

		public void SetTilt(float pitchAngle, bool isInstant)
		{
			if (!(firstPersonCamera == null))
			{
				firstPersonCamera.SetPitch(pitchAngle, isInstant);
			}
		}

		public override void SetHeadTurnTarget(Transform _headTurnTarget, Vector3 _headTurnTargetOffset, bool isInstant, HeadFacing _headFacing = HeadFacing.Manual)
		{
			if (_headFacing == HeadFacing.Hotspot && lockHotspotHeadTurning)
			{
				ClearHeadTurnTarget(false, HeadFacing.Hotspot);
			}
			else
			{
				base.SetHeadTurnTarget(_headTurnTarget, _headTurnTargetOffset, isInstant, _headFacing);
			}
		}

		public void SetHotspotHeadTurnLock(bool state)
		{
			lockHotspotHeadTurning = state;
		}

		public PlayerData SavePlayerData(PlayerData playerData)
		{
			playerData.playerID = ID;
			playerData.playerLocX = base.transform.position.x;
			playerData.playerLocY = base.transform.position.y;
			playerData.playerLocZ = base.transform.position.z;
			playerData.playerRotY = base.TransformRotation.eulerAngles.y;
			playerData.inCustomCharState = charState == CharState.Custom && GetAnimator() != null && (bool)GetAnimator().GetComponent<RememberAnimator>();
			playerData.playerWalkSpeed = walkSpeedScale;
			playerData.playerRunSpeed = runSpeedScale;
			playerData = GetAnimEngine().SavePlayerData(playerData, this);
			playerData.playerWalkSound = AssetLoader.GetAssetInstanceID(walkSound);
			playerData.playerRunSound = AssetLoader.GetAssetInstanceID(runSound);
			playerData.playerPortraitGraphic = AssetLoader.GetAssetInstanceID(portraitIcon.texture);
			playerData.playerSpeechLabel = GetName();
			playerData.playerDisplayLineID = displayLineID;
			playerData.playerLockDirection = lockDirection;
			playerData.playerLockScale = lockScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<FollowSortingMap>())
			{
				playerData.playerLockSorting = spriteChild.GetComponent<FollowSortingMap>().lockSorting;
			}
			else if ((bool)GetComponent<FollowSortingMap>())
			{
				playerData.playerLockSorting = GetComponent<FollowSortingMap>().lockSorting;
			}
			else
			{
				playerData.playerLockSorting = false;
			}
			playerData.playerSpriteDirection = spriteDirection;
			playerData.playerSpriteScale = spriteScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<Renderer>())
			{
				playerData.playerSortingOrder = spriteChild.GetComponent<Renderer>().sortingOrder;
				playerData.playerSortingLayer = spriteChild.GetComponent<Renderer>().sortingLayerName;
			}
			else if ((bool)GetComponent<Renderer>())
			{
				playerData.playerSortingOrder = GetComponent<Renderer>().sortingOrder;
				playerData.playerSortingLayer = GetComponent<Renderer>().sortingLayerName;
			}
			playerData.playerActivePath = 0;
			playerData.lastPlayerActivePath = 0;
			if ((bool)GetPath())
			{
				playerData.playerTargetNode = GetTargetNode();
				playerData.playerPrevNode = GetPreviousNode();
				playerData.playerIsRunning = base.isRunning;
				playerData.playerPathAffectY = activePath.affectY;
				if ((bool)GetComponent<Paths>() && GetPath() == GetComponent<Paths>())
				{
					playerData.playerPathData = Serializer.CreatePathData(GetComponent<Paths>());
					playerData.playerLockedPath = false;
				}
				else
				{
					playerData.playerPathData = string.Empty;
					playerData.playerActivePath = Serializer.GetConstantID(GetPath().gameObject);
					playerData.playerLockedPath = lockedPath;
				}
			}
			if ((bool)GetLastPath())
			{
				playerData.lastPlayerTargetNode = GetLastTargetNode();
				playerData.lastPlayerPrevNode = GetLastPrevNode();
				playerData.lastPlayerActivePath = Serializer.GetConstantID(GetLastPath().gameObject);
			}
			playerData.playerIgnoreGravity = ignoreGravity;
			playerData.playerLockHotspotHeadTurning = lockHotspotHeadTurning;
			if (headFacing == HeadFacing.Manual && headTurnTarget != null)
			{
				playerData.isHeadTurning = true;
				playerData.headTargetID = Serializer.GetConstantID(headTurnTarget);
				if (playerData.headTargetID == 0)
				{
					ACDebug.LogWarning(string.Concat("The Player's head-turning target Transform, ", headTurnTarget, ", was not saved because it has no Constant ID"), base.gameObject);
				}
				playerData.headTargetX = headTurnTargetOffset.x;
				playerData.headTargetY = headTurnTargetOffset.y;
				playerData.headTargetZ = headTurnTargetOffset.z;
			}
			else
			{
				playerData.isHeadTurning = false;
				playerData.headTargetID = 0;
				playerData.headTargetX = 0f;
				playerData.headTargetY = 0f;
				playerData.headTargetZ = 0f;
			}
			FollowSortingMap componentInChildren = GetComponentInChildren<FollowSortingMap>();
			if (componentInChildren != null)
			{
				playerData.followSortingMap = componentInChildren.followSortingMap;
				if (!playerData.followSortingMap && componentInChildren.GetSortingMap() != null)
				{
					if (componentInChildren.GetSortingMap().GetComponent<ConstantID>() != null)
					{
						playerData.customSortingMapID = componentInChildren.GetSortingMap().GetComponent<ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning("The Player's SortingMap, " + componentInChildren.GetSortingMap().name + ", was not saved because it has no Constant ID", base.gameObject);
						playerData.customSortingMapID = 0;
					}
				}
				else
				{
					playerData.customSortingMapID = 0;
				}
			}
			else
			{
				playerData.followSortingMap = false;
				playerData.customSortingMapID = 0;
			}
			if (!IsLocalPlayer())
			{
				playerData = KickStarter.levelStorage.SavePlayerData(this, playerData);
			}
			return playerData;
		}

		public bool IsLocalPlayer()
		{
			return ID <= -2;
		}

		public void LoadPlayerData(PlayerData playerData, bool justAnimationData = false)
		{
			if (!justAnimationData)
			{
				charState = (playerData.inCustomCharState ? CharState.Custom : CharState.Idle);
				Teleport(new Vector3(playerData.playerLocX, playerData.playerLocY, playerData.playerLocZ));
				SetRotation(playerData.playerRotY);
				SetMoveDirectionAsForward();
			}
			walkSpeedScale = playerData.playerWalkSpeed;
			runSpeedScale = playerData.playerRunSpeed;
			GetAnimEngine().LoadPlayerData(playerData, this);
			walkSound = AssetLoader.RetrieveAsset(walkSound, playerData.playerWalkSound);
			runSound = AssetLoader.RetrieveAsset(runSound, playerData.playerRunSound);
			portraitIcon.ReplaceTexture(AssetLoader.RetrieveAsset(portraitIcon.texture, playerData.playerPortraitGraphic));
			if (!string.IsNullOrEmpty(playerData.playerSpeechLabel))
			{
				SetName(playerData.playerSpeechLabel, playerData.playerDisplayLineID);
			}
			speechLabel = playerData.playerSpeechLabel;
			lockDirection = playerData.playerLockDirection;
			lockScale = playerData.playerLockScale;
			if ((bool)spriteChild && (bool)spriteChild.GetComponent<FollowSortingMap>())
			{
				spriteChild.GetComponent<FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else if ((bool)GetComponent<FollowSortingMap>())
			{
				GetComponent<FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else
			{
				ReleaseSorting();
			}
			if (playerData.playerLockDirection)
			{
				spriteDirection = playerData.playerSpriteDirection;
			}
			if (playerData.playerLockScale)
			{
				spriteScale = playerData.playerSpriteScale;
			}
			if (playerData.playerLockSorting)
			{
				if ((bool)spriteChild && (bool)spriteChild.GetComponent<Renderer>())
				{
					spriteChild.GetComponent<Renderer>().sortingOrder = playerData.playerSortingOrder;
					spriteChild.GetComponent<Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
				else if ((bool)GetComponent<Renderer>())
				{
					GetComponent<Renderer>().sortingOrder = playerData.playerSortingOrder;
					GetComponent<Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
			}
			if (!justAnimationData)
			{
				Halt();
				ForceIdle();
			}
			if (!string.IsNullOrEmpty(playerData.playerPathData) && (bool)GetComponent<Paths>())
			{
				Paths component = GetComponent<Paths>();
				component = Serializer.RestorePathData(component, playerData.playerPathData);
				SetPath(component, playerData.playerTargetNode, playerData.playerPrevNode, playerData.playerPathAffectY);
				base.isRunning = playerData.playerIsRunning;
				lockedPath = false;
			}
			else if (playerData.playerActivePath != 0)
			{
				Paths paths = Serializer.returnComponent<Paths>(playerData.playerActivePath);
				if ((bool)paths)
				{
					lockedPath = playerData.playerLockedPath;
					if (lockedPath)
					{
						SetLockedPath(paths);
					}
					else
					{
						SetPath(paths, playerData.playerTargetNode, playerData.playerPrevNode);
					}
				}
				else
				{
					Halt();
					ForceIdle();
				}
			}
			else
			{
				Halt();
				ForceIdle();
			}
			if (playerData.lastPlayerActivePath != 0)
			{
				Paths paths2 = Serializer.returnComponent<Paths>(playerData.lastPlayerActivePath);
				if ((bool)paths2)
				{
					SetLastPath(paths2, playerData.lastPlayerTargetNode, playerData.lastPlayerPrevNode);
				}
			}
			lockHotspotHeadTurning = playerData.playerLockHotspotHeadTurning;
			if (playerData.isHeadTurning)
			{
				ConstantID constantID = Serializer.returnComponent<ConstantID>(playerData.headTargetID);
				if (constantID != null)
				{
					SetHeadTurnTarget(constantID.transform, new Vector3(playerData.headTargetX, playerData.headTargetY, playerData.headTargetZ), true);
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
			ignoreGravity = playerData.playerIgnoreGravity;
			if (GetComponentsInChildren<FollowSortingMap>() != null)
			{
				FollowSortingMap[] componentsInChildren = GetComponentsInChildren<FollowSortingMap>();
				SortingMap sortingMap = Serializer.returnComponent<SortingMap>(playerData.customSortingMapID);
				FollowSortingMap[] array = componentsInChildren;
				foreach (FollowSortingMap followSortingMap in array)
				{
					followSortingMap.followSortingMap = playerData.followSortingMap;
					if (!playerData.followSortingMap && sortingMap != null)
					{
						followSortingMap.SetSortingMap(sortingMap);
					}
					else
					{
						followSortingMap.SetSortingMap(KickStarter.sceneSettings.sortingMap);
					}
				}
			}
			ignoreGravity = playerData.playerIgnoreGravity;
			if (!IsLocalPlayer())
			{
				KickStarter.levelStorage.LoadPlayerData(this, playerData);
			}
		}

		public virtual void Hide()
		{
			SkinnedMeshRenderer[] array = skinnedMeshRenderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				skinnedMeshRenderer.enabled = false;
			}
		}

		public virtual void Show()
		{
			SkinnedMeshRenderer[] array = skinnedMeshRenderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				skinnedMeshRenderer.enabled = true;
			}
		}

		public void RepositionToTransform(Transform otherTransform)
		{
			Teleport(otherTransform.position);
			NPC component = otherTransform.gameObject.GetComponent<NPC>();
			Quaternion rotation = ((!(component != null)) ? otherTransform.rotation : component.TransformRotation);
			SetRotation(rotation);
			base.transform.localScale = otherTransform.localScale;
		}

		public NPC GetRuntimeAssociatedNPC()
		{
			if (associatedNPCPrefab != null)
			{
				ConstantID component = associatedNPCPrefab.GetComponent<ConstantID>();
				if (component != null)
				{
					NPC nPC = Serializer.returnComponent<NPC>(component.constantID);
					if (nPC != null && nPC.gameObject.activeInHierarchy)
					{
						return nPC;
					}
				}
			}
			return null;
		}
	}
}
