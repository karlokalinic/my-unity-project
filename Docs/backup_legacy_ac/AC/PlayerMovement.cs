using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_movement.html")]
	public class PlayerMovement : MonoBehaviour
	{
		protected FirstPersonCamera firstPersonCamera;

		protected float moveStraightToCursorUpdateTime;

		protected float moveStraightToCursorHoldTime;

		protected bool movingFromHold;

		protected GameObject clickPrefabInstance;

		public void OnStart()
		{
			AssignFPCamera();
		}

		public void UpdateFPCamera()
		{
			if (firstPersonCamera != null)
			{
				firstPersonCamera._UpdateFPCamera();
			}
		}

		public Transform AssignFPCamera()
		{
			if ((bool)KickStarter.player)
			{
				firstPersonCamera = KickStarter.player.GetComponentInChildren<FirstPersonCamera>();
				if (firstPersonCamera == null && KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson && KickStarter.player.FirstPersonCamera == null)
				{
					ACDebug.LogWarning("Could not find a FirstPersonCamera script on the Player - one is necessary for first-person movement.", KickStarter.player);
				}
				if (firstPersonCamera != null)
				{
					return firstPersonCamera.transform;
				}
			}
			return null;
		}

		public void UpdatePlayerMovement()
		{
			if (!KickStarter.settingsManager || !KickStarter.player || !KickStarter.playerInput || !KickStarter.playerInteraction)
			{
				return;
			}
			UpdateMoveStraightToCursorTime();
			if (KickStarter.playerInput.activeArrows != null || ((KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick || KickStarter.settingsManager.movementMethod == MovementMethod.Drag || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor) && !KickStarter.playerInput.IsMouseOnScreen()))
			{
				return;
			}
			if (KickStarter.settingsManager.disableMovementWhenInterationMenusAreOpen && (bool)KickStarter.player && KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.playerMenus.IsInteractionMenuOn())
			{
				KickStarter.player.Halt();
			}
			else
			{
				if (UnityUIBlocksClick())
				{
					return;
				}
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick && !KickStarter.playerMenus.IsInteractionMenuOn() && !KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerInteraction.IsMouseOverHotspot())
				{
					if (KickStarter.playerInteraction.GetHotspotMovingTo() != null)
					{
						KickStarter.playerInteraction.StopMovingToHotspot();
					}
					KickStarter.playerInteraction.DeselectHotspot();
				}
				if (KickStarter.playerInteraction.GetHotspotMovingTo() != null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick && KickStarter.playerInput.GetMoveKeys() != Vector2.zero)
				{
					KickStarter.playerInteraction.StopMovingToHotspot();
				}
				switch (KickStarter.settingsManager.movementMethod)
				{
				case MovementMethod.None:
					break;
				case MovementMethod.Direct:
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.settingsManager.directTouchScreen == DirectTouchScreen.DragBased)
					{
						DragPlayer(true, KickStarter.playerInput.GetMoveKeys());
					}
					else if (KickStarter.player.GetPath() == null || !KickStarter.player.IsLockedToPath())
					{
						DirectControlPlayer(false, KickStarter.playerInput.GetMoveKeys());
					}
					else
					{
						DirectControlPlayerPath(KickStarter.playerInput.GetMoveKeys());
					}
					break;
				case MovementMethod.Drag:
					DragPlayer(true, KickStarter.playerInput.GetMoveKeys());
					break;
				case MovementMethod.StraightToCursor:
					MoveStraightToCursor();
					break;
				case MovementMethod.PointAndClick:
					PointControlPlayer();
					break;
				case MovementMethod.FirstPerson:
					if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						if (KickStarter.settingsManager.firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToTurnAndTwoTouchesToMove)
						{
							if (Input.touchCount == 1)
							{
								FirstPersonControlPlayer();
								DragPlayerLook();
							}
							else
							{
								DragPlayerTouch(KickStarter.playerInput.GetMoveKeys());
							}
						}
						else if (KickStarter.settingsManager.firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToMoveAndTurn)
						{
							FirstPersonControlPlayer();
							DragPlayer(false, KickStarter.playerInput.GetMoveKeys());
						}
						else if (KickStarter.settingsManager.firstPersonTouchScreen == FirstPersonTouchScreen.TouchControlsTurningOnly)
						{
							FirstPersonControlPlayer();
							DragPlayerLook();
						}
						else if (KickStarter.settingsManager.firstPersonTouchScreen == FirstPersonTouchScreen.CustomInput)
						{
							FirstPersonControlPlayer();
							DirectControlPlayer(true, KickStarter.playerInput.GetMoveKeys());
						}
					}
					else
					{
						FirstPersonControlPlayer();
						DirectControlPlayer(true, KickStarter.playerInput.GetMoveKeys());
					}
					break;
				}
			}
		}

		protected void UpdateMoveStraightToCursorTime()
		{
			if (moveStraightToCursorUpdateTime > 0f)
			{
				moveStraightToCursorUpdateTime -= Time.deltaTime;
				if (moveStraightToCursorUpdateTime < 0f)
				{
					moveStraightToCursorUpdateTime = 0f;
				}
			}
			if (KickStarter.settingsManager.clickHoldSeparationStraight > 0f && KickStarter.settingsManager.singleTapStraight)
			{
				if (KickStarter.playerInput.GetMouseState() == MouseState.Normal)
				{
					moveStraightToCursorHoldTime = KickStarter.settingsManager.clickHoldSeparationStraight;
				}
				if (moveStraightToCursorHoldTime > 0f)
				{
					moveStraightToCursorHoldTime -= Time.deltaTime;
					if (moveStraightToCursorHoldTime < 0f)
					{
						moveStraightToCursorHoldTime = 0f;
					}
				}
			}
			else
			{
				moveStraightToCursorHoldTime = 0f;
			}
		}

		protected void MoveStraightToCursor()
		{
			if (KickStarter.playerInput.AllDirectionsLocked())
			{
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.StartDecelerating();
				}
				return;
			}
			if (KickStarter.playerInput.GetDragState() == DragState.None)
			{
				KickStarter.playerInput.ResetDragMovement();
				if (KickStarter.player.charState == CharState.Move && KickStarter.player.GetPath() == null)
				{
					KickStarter.player.StartDecelerating();
				}
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick && KickStarter.settingsManager.singleTapStraight)
			{
				movingFromHold = false;
				if (KickStarter.settingsManager.singleTapStraightPathfind)
				{
					PointControlPlayer();
					return;
				}
				Vector3 straightToCursorClickPoint = GetStraightToCursorClickPoint();
				Vector3 vector = straightToCursorClickPoint - KickStarter.player.transform.position;
				if (!(straightToCursorClickPoint != Vector3.zero))
				{
					return;
				}
				if (vector.magnitude > KickStarter.settingsManager.GetDestinationThreshold())
				{
					if (SceneSettings.IsUnity2D())
					{
						vector = new Vector3(vector.x, 0f, vector.y);
					}
					bool run = vector.magnitude > KickStarter.settingsManager.dragRunThreshold;
					if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
					{
						run = true;
					}
					else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
					{
						run = false;
					}
					List<Vector3> list = new List<Vector3>();
					list.Add(straightToCursorClickPoint);
					PointMovePlayer(list.ToArray(), run);
				}
				else if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.StartDecelerating();
				}
			}
			else if (KickStarter.playerInput.GetDragState() == DragState.Player && moveStraightToCursorHoldTime <= 0f && (!KickStarter.settingsManager.singleTapStraight || KickStarter.playerInput.CanClick()))
			{
				Vector3 straightToCursorClickPoint2 = GetStraightToCursorClickPoint();
				Vector3 direction = straightToCursorClickPoint2 - KickStarter.player.transform.position;
				if (straightToCursorClickPoint2 != Vector3.zero)
				{
					if (direction.magnitude > KickStarter.settingsManager.GetDestinationThreshold())
					{
						if (SceneSettings.IsUnity2D())
						{
							direction = new Vector3(direction.x, 0f, direction.y);
						}
						bool flag = direction.magnitude > KickStarter.settingsManager.dragRunThreshold;
						if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
						{
							flag = true;
						}
						else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
						{
							flag = false;
						}
						if (KickStarter.settingsManager.pathfindUpdateFrequency > 0f)
						{
							if (moveStraightToCursorUpdateTime <= 0f && (!movingFromHold || !KickStarter.player.IsPathfinding() || !((straightToCursorClickPoint2 - KickStarter.player.GetTargetPosition(true)).magnitude < KickStarter.settingsManager.GetDestinationThreshold())))
							{
								Vector3[] pointsArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(KickStarter.player.transform.position, straightToCursorClickPoint2, KickStarter.player);
								PointMovePlayer(pointsArray, flag);
								moveStraightToCursorUpdateTime = KickStarter.settingsManager.pathfindUpdateFrequency;
								movingFromHold = true;
							}
						}
						else
						{
							KickStarter.player.isRunning = flag;
							KickStarter.player.charState = CharState.Move;
							KickStarter.player.SetLookDirection(direction, false);
							KickStarter.player.SetMoveDirectionAsForward();
							movingFromHold = true;
						}
					}
					else if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.StartDecelerating();
						movingFromHold = false;
					}
					if ((bool)KickStarter.player.GetPath() && (KickStarter.settingsManager.pathfindUpdateFrequency <= 0f || KickStarter.playerInput.GetMouseState() != MouseState.HeldDown))
					{
						KickStarter.player.EndPath();
						movingFromHold = false;
					}
				}
				else
				{
					if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.StartDecelerating();
						movingFromHold = false;
					}
					if ((bool)KickStarter.player.GetPath())
					{
						KickStarter.player.EndPath();
						movingFromHold = false;
					}
				}
			}
			else if (KickStarter.player.charState == CharState.Move || KickStarter.player.IsPathfinding())
			{
				if (movingFromHold && moveStraightToCursorHoldTime > 0f)
				{
					if (KickStarter.player.charState == CharState.Move)
					{
						KickStarter.player.StartDecelerating();
					}
					if ((bool)KickStarter.player.GetPath())
					{
						KickStarter.player.EndPath();
					}
				}
			}
			else
			{
				movingFromHold = false;
			}
		}

		protected Vector3 GetStraightToCursorClickPoint()
		{
			Vector2 mousePosition = KickStarter.playerInput.GetMousePosition();
			Vector3 vector = ClickPoint(mousePosition);
			if (vector == Vector3.zero)
			{
				if (KickStarter.settingsManager.walkableClickRange > 0f && (float)ACScreen.height * KickStarter.settingsManager.walkableClickRange > 1f)
				{
					float num = 100f;
					float num2 = (float)ACScreen.height / num;
					if (KickStarter.settingsManager.navMeshSearchDirection == NavMeshSearchDirection.StraightDownFromCursor)
					{
						for (float num3 = 1f; num3 < (float)ACScreen.height * KickStarter.settingsManager.walkableClickRange; num3 += num2)
						{
							vector = ClickPoint(new Vector2(mousePosition.x, mousePosition.y - num3));
							if (vector != Vector3.zero)
							{
								return vector;
							}
						}
					}
					for (float num4 = 1f; num4 < (float)ACScreen.height * KickStarter.settingsManager.walkableClickRange; num4 += num2)
					{
						vector = ClickPoint(new Vector2(mousePosition.x, mousePosition.y + num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x, mousePosition.y - num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x - num4, mousePosition.y));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x + num4, mousePosition.y));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x - num4, mousePosition.y - num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x + num4, mousePosition.y - num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x - num4, mousePosition.y + num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
						vector = ClickPoint(new Vector2(mousePosition.x + num4, mousePosition.y + num4));
						if (vector != Vector3.zero)
						{
							return vector;
						}
					}
				}
				return Vector3.zero;
			}
			return vector;
		}

		public Vector3 ClickPoint(Vector2 screenPosition, bool onNavMesh = false)
		{
			if (KickStarter.navigationManager.Is2D())
			{
				RaycastHit2D raycastHit2D;
				if (KickStarter.mainCamera.IsOrthographic())
				{
					raycastHit2D = ((!onNavMesh) ? UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(new Vector2(screenPosition.x, screenPosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength) : UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(new Vector2(screenPosition.x, screenPosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer)));
				}
				else
				{
					Vector3 position = screenPosition;
					position.z = KickStarter.player.transform.position.z - KickStarter.CameraMain.transform.position.z;
					raycastHit2D = ((!onNavMesh) ? UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength) : UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer)));
				}
				if (raycastHit2D.collider != null)
				{
					return raycastHit2D.point;
				}
			}
			else
			{
				Ray ray = KickStarter.CameraMain.ScreenPointToRay(screenPosition);
				RaycastHit hitInfo = default(RaycastHit);
				if (onNavMesh)
				{
					if ((bool)KickStarter.settingsManager && (bool)KickStarter.sceneSettings && Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer)))
					{
						return hitInfo.point;
					}
				}
				else if ((bool)KickStarter.settingsManager && (bool)KickStarter.sceneSettings && Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength))
				{
					return hitInfo.point;
				}
			}
			return Vector3.zero;
		}

		protected void DragPlayer(bool doRotation, Vector2 moveKeys)
		{
			if (KickStarter.playerInput.GetDragState() == DragState.None)
			{
				KickStarter.playerInput.ResetDragMovement();
				if (KickStarter.player.charState == CharState.Move && KickStarter.playerInteraction.GetHotspotMovingTo() == null)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			if (KickStarter.playerInput.GetDragState() != DragState.Player)
			{
				return;
			}
			Vector3 zero = Vector3.zero;
			zero = ((!SceneSettings.IsTopDown()) ? (moveKeys.y * KickStarter.mainCamera.ForwardVector() + moveKeys.x * KickStarter.mainCamera.RightVector()) : (moveKeys.y * Vector3.forward + moveKeys.x * Vector3.right));
			if (KickStarter.playerInput.IsDragMoveSpeedOverWalkThreshold())
			{
				KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
				KickStarter.player.charState = CharState.Move;
				if (doRotation)
				{
					KickStarter.player.SetLookDirection(zero, false);
					KickStarter.player.SetMoveDirectionAsForward();
				}
				else if (KickStarter.playerInput.GetDragVector().y < 0f)
				{
					KickStarter.player.SetMoveDirectionAsForward();
				}
				else
				{
					KickStarter.player.SetMoveDirectionAsBackward();
				}
			}
			else if (KickStarter.player.charState == CharState.Move && KickStarter.playerInteraction.GetHotspotMovingTo() == null)
			{
				KickStarter.player.StartDecelerating();
			}
		}

		protected void DragPlayerTouch(Vector2 moveKeys)
		{
			if (KickStarter.playerInput.GetDragState() == DragState.None)
			{
				KickStarter.playerInput.ResetDragMovement();
				if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			if (KickStarter.playerInput.GetDragState() == DragState.Player)
			{
				Vector3 vector = moveKeys.y * KickStarter.mainCamera.ForwardVector() + moveKeys.x * KickStarter.mainCamera.RightVector();
				if (KickStarter.playerInput.IsDragMoveSpeedOverWalkThreshold())
				{
					KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirection(KickStarter.player.transform.position + vector);
				}
				else if (KickStarter.player.charState == CharState.Move && KickStarter.playerInteraction.GetHotspotMovingTo() == null)
				{
					KickStarter.player.StartDecelerating();
				}
			}
		}

		protected void DirectControlPlayer(bool isFirstPerson, Vector2 moveKeys)
		{
			KickStarter.player.CancelPathfindRecalculations();
			if (KickStarter.settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
			{
				if (moveKeys != Vector2.zero)
				{
					Vector3 zero = Vector3.zero;
					if (SceneSettings.IsTopDown())
					{
						zero = moveKeys.y * Vector3.forward + moveKeys.x * Vector3.right;
					}
					else if (!isFirstPerson && KickStarter.settingsManager.directMovementPerspective && SceneSettings.CameraPerspective == CameraPerspective.ThreeD)
					{
						Vector3 normalized = (KickStarter.player.transform.position - KickStarter.CameraMain.transform.position).normalized;
						Vector3 vector = -Vector3.Cross(normalized, KickStarter.CameraMain.transform.up);
						zero = moveKeys.y * normalized + moveKeys.x * vector;
					}
					else
					{
						zero = moveKeys.y * KickStarter.mainCamera.ForwardVector() + moveKeys.x * KickStarter.mainCamera.RightVector();
					}
					KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
					KickStarter.player.charState = CharState.Move;
					if (!KickStarter.playerInput.cameraLockSnap)
					{
						if (isFirstPerson)
						{
							KickStarter.player.SetMoveDirection(zero);
							return;
						}
						KickStarter.player.SetLookDirection(zero, KickStarter.settingsManager.directTurnsInstantly);
						KickStarter.player.SetMoveDirectionAsForward();
					}
				}
				else if (KickStarter.player.charState == CharState.Move && KickStarter.playerInteraction.GetHotspotMovingTo() == null)
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
			}
			else
			{
				if (KickStarter.settingsManager.directMovementType != DirectMovementType.TankControls)
				{
					return;
				}
				if (KickStarter.settingsManager.magnitudeAffectsDirect || isFirstPerson)
				{
					if (moveKeys.x < 0f)
					{
						KickStarter.player.TankTurnLeft(0f - moveKeys.x);
					}
					else if (moveKeys.x > 0f)
					{
						KickStarter.player.TankTurnRight(moveKeys.x);
					}
					else
					{
						KickStarter.player.StopTankTurning();
					}
				}
				else if (moveKeys.x < -0.3f)
				{
					KickStarter.player.TankTurnLeft();
				}
				else if (moveKeys.x > 0.3f)
				{
					KickStarter.player.TankTurnRight();
				}
				else
				{
					KickStarter.player.StopTankTurning();
				}
				if (moveKeys.y > 0f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsForward();
				}
				else if (moveKeys.y < 0f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
					KickStarter.player.charState = CharState.Move;
					KickStarter.player.SetMoveDirectionAsBackward();
				}
				else if (KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.charState = CharState.Decelerate;
					if (KickStarter.player.IsReversing())
					{
						KickStarter.player.SetMoveDirectionAsBackward();
					}
					else
					{
						KickStarter.player.SetMoveDirectionAsForward();
					}
				}
			}
		}

		protected void DirectControlPlayerPath(Vector2 moveKeys)
		{
			if (moveKeys != Vector2.zero)
			{
				Vector3 zero = Vector3.zero;
				zero = ((!SceneSettings.IsTopDown()) ? (moveKeys.y * KickStarter.mainCamera.ForwardVector() + moveKeys.x * KickStarter.mainCamera.RightVector()) : (moveKeys.y * Vector3.forward + moveKeys.x * Vector3.right));
				if (Vector3.Dot(zero, KickStarter.player.GetMoveDirection()) > 0f)
				{
					KickStarter.player.isRunning = KickStarter.playerInput.IsPlayerControlledRunning();
					KickStarter.player.charState = CharState.Move;
				}
			}
			else if (KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.StartDecelerating();
			}
		}

		protected void PointControlPlayer()
		{
			if (KickStarter.playerInput.IsCursorLocked() || !KickStarter.mainCamera.IsPointInCamera(KickStarter.playerInput.GetMousePosition()))
			{
				return;
			}
			if (KickStarter.playerInput.AllDirectionsLocked())
			{
				if (KickStarter.player.GetPath() == null && KickStarter.player.charState == CharState.Move)
				{
					KickStarter.player.StartDecelerating();
				}
			}
			else if ((KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.DoubleClick) && !KickStarter.playerMenus.IsInteractionMenuOn() && !KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerInteraction.IsMouseOverHotspot() && (bool)KickStarter.playerCursor)
			{
				if (KickStarter.playerCursor.GetSelectedCursor() < 0)
				{
					if ((KickStarter.settingsManager.doubleClickMovement == DoubleClickMovement.RequiredToWalk && KickStarter.playerInput.GetMouseState() == MouseState.SingleClick) || KickStarter.playerInput.GetDragState() == DragState.Moveable || (KickStarter.runtimeInventory.SelectedItem != null && !KickStarter.settingsManager.canMoveWhenActive && KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick && !KickStarter.settingsManager.inventoryDisableLeft))
					{
						return;
					}
					bool run = false;
					if (KickStarter.playerInput.GetMouseState() == MouseState.DoubleClick && KickStarter.settingsManager.doubleClickMovement == DoubleClickMovement.MakesPlayerRun)
					{
						run = true;
					}
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.playerMenus != null)
					{
						KickStarter.playerMenus.CloseInteractionMenus();
					}
					Vector3 vector = KickStarter.playerInput.GetMousePosition();
					if (((!SceneSettings.IsUnity2D() || SearchForNavMesh2D(vector, Vector2.zero, run)) && (SceneSettings.IsUnity2D() || RaycastNavMesh(vector, run))) || !(KickStarter.settingsManager.walkableClickRange > 0f) || !((float)ACScreen.height * KickStarter.settingsManager.walkableClickRange > 1f))
					{
						return;
					}
					float num = 100f;
					float num2 = (float)ACScreen.height / num;
					if (KickStarter.settingsManager.navMeshSearchDirection == NavMeshSearchDirection.StraightDownFromCursor)
					{
						if (SceneSettings.IsUnity2D())
						{
							if (SearchForNavMesh2D(vector, -Vector2.up, run))
							{
								return;
							}
						}
						else
						{
							for (float num3 = 1f; num3 < (float)ACScreen.height * KickStarter.settingsManager.walkableClickRange; num3 += num2)
							{
								if (RaycastNavMesh(new Vector2(vector.x, vector.y - num3), run))
								{
									return;
								}
							}
						}
					}
					for (float num4 = 1f; num4 < (float)ACScreen.height * KickStarter.settingsManager.walkableClickRange && !RaycastNavMesh(new Vector2(vector.x, vector.y + num4), run) && (KickStarter.settingsManager.navMeshSearchDirection != NavMeshSearchDirection.RadiallyOutwardsFromCursor || !RaycastNavMesh(new Vector2(vector.x, vector.y - num4), run)) && !RaycastNavMesh(new Vector2(vector.x - num4, vector.y), run) && !RaycastNavMesh(new Vector2(vector.x + num4, vector.y), run) && !RaycastNavMesh(new Vector2(vector.x - num4, vector.y - num4), run) && !RaycastNavMesh(new Vector2(vector.x + num4, vector.y - num4), run) && !RaycastNavMesh(new Vector2(vector.x - num4, vector.y + num4), run) && !RaycastNavMesh(new Vector2(vector.x + num4, vector.y + num4), run); num4 += num2)
					{
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.autoCycleWhenInteract)
				{
					KickStarter.playerCursor.ResetSelectedCursor();
				}
			}
			else if (KickStarter.player.GetPath() == null && KickStarter.player.charState == CharState.Move)
			{
				KickStarter.player.StartDecelerating();
			}
		}

		protected bool ProcessHit(Vector3 hitPoint, GameObject hitObject, bool run)
		{
			if (hitObject.layer != LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer))
			{
				return false;
			}
			if (Vector3.Distance(hitPoint, KickStarter.player.transform.position) < KickStarter.settingsManager.GetDestinationThreshold())
			{
				return true;
			}
			bool flag = !run;
			if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
			{
				run = true;
			}
			else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				run = false;
			}
			else if (Vector3.Distance(hitPoint, KickStarter.player.transform.position) < KickStarter.player.runDistanceThreshold)
			{
				run = false;
			}
			Vector3[] pointsArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(KickStarter.player.transform.position, hitPoint, KickStarter.player);
			PointMovePlayer(pointsArray, run);
			if (flag)
			{
				switch (KickStarter.settingsManager.clickMarkerPosition)
				{
				case ClickMarkerPosition.ColliderContactPoint:
					ShowClick(hitPoint);
					break;
				case ClickMarkerPosition.PlayerDestination:
					if (pointsArray.Length > 0)
					{
						ShowClick(pointsArray[pointsArray.Length - 1]);
					}
					break;
				}
			}
			return true;
		}

		protected void PointMovePlayer(Vector3[] pointArray, bool run)
		{
			KickStarter.eventManager.Call_OnPointAndClick(pointArray, run);
			KickStarter.player.MoveAlongPoints(pointArray, run);
		}

		protected bool SearchForNavMesh2D(Vector2 mousePosition, Vector2 direction, bool run)
		{
			RaycastHit2D raycastHit2D;
			if (KickStarter.mainCamera.IsOrthographic())
			{
				raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(mousePosition), direction, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer));
			}
			else
			{
				Vector3 position = mousePosition;
				position.z = KickStarter.player.transform.position.z - KickStarter.CameraMain.transform.position.z;
				raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), direction, KickStarter.settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.navMeshLayer));
			}
			if (raycastHit2D.collider != null)
			{
				return ProcessHit(raycastHit2D.point, raycastHit2D.collider.gameObject, run);
			}
			return false;
		}

		protected bool RaycastNavMesh(Vector3 mousePosition, bool run)
		{
			if (KickStarter.settingsManager.ignoreOffScreenNavMesh && (mousePosition.x < 0f || mousePosition.y < 0f || mousePosition.x > (float)ACScreen.width || mousePosition.y > (float)ACScreen.height))
			{
				return false;
			}
			if (KickStarter.navigationManager.Is2D())
			{
				RaycastHit2D raycastHit2D;
				if (KickStarter.mainCamera.IsOrthographic())
				{
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(new Vector2(mousePosition.x, mousePosition.y)), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength);
				}
				else
				{
					Vector3 position = mousePosition;
					position.z = KickStarter.player.transform.position.z - KickStarter.CameraMain.transform.position.z;
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength);
				}
				if (raycastHit2D.collider != null)
				{
					return ProcessHit(raycastHit2D.point, raycastHit2D.collider.gameObject, run);
				}
			}
			else
			{
				Ray ray = KickStarter.CameraMain.ScreenPointToRay(mousePosition);
				RaycastHit hitInfo = default(RaycastHit);
				if ((bool)KickStarter.settingsManager && (bool)KickStarter.sceneSettings && Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.navMeshRaycastLength))
				{
					return ProcessHit(hitInfo.point, hitInfo.collider.gameObject, run);
				}
			}
			return false;
		}

		protected void ShowClick(Vector3 clickPoint)
		{
			if ((bool)KickStarter.settingsManager && (bool)KickStarter.settingsManager.clickPrefab)
			{
				if (clickPrefabInstance != null && clickPrefabInstance.activeSelf)
				{
					KickStarter.sceneChanger.ScheduleForDeletion(clickPrefabInstance);
				}
				Transform transform = Object.Instantiate(KickStarter.settingsManager.clickPrefab, clickPoint, Quaternion.identity);
				clickPrefabInstance = transform.gameObject;
			}
		}

		protected void FirstPersonControlPlayer()
		{
			Vector2 freeAim = KickStarter.playerInput.GetFreeAim();
			if (freeAim.magnitude > KickStarter.settingsManager.dragWalkThreshold / 10f)
			{
				freeAim.Normalize();
				freeAim *= KickStarter.settingsManager.dragWalkThreshold / 10f;
			}
			float y = KickStarter.player.TransformRotation.eulerAngles.y;
			if (firstPersonCamera != null)
			{
				y += freeAim.x * firstPersonCamera.sensitivity.x;
				firstPersonCamera.IncreasePitch(0f - freeAim.y);
			}
			else
			{
				y += freeAim.x * 15f;
			}
			Quaternion rotation = Quaternion.AngleAxis(y, Vector3.up);
			KickStarter.player.SetRotation(rotation);
			KickStarter.player.ForceTurnFloat(freeAim.x * 2f);
		}

		protected void DragPlayerLook()
		{
			if (!KickStarter.playerInput.AllDirectionsLocked() && KickStarter.playerInput.GetMouseState() != MouseState.Normal && !KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerMenus.IsInteractionMenuOn() && (KickStarter.playerInput.GetMouseState() == MouseState.RightClick || !KickStarter.playerInteraction.IsMouseOverHotspot()) && KickStarter.playerInput.GetMouseState() == MouseState.SingleClick)
			{
				KickStarter.playerInteraction.DeselectHotspot();
			}
		}

		protected virtual bool UnityUIBlocksClick()
		{
			if (KickStarter.settingsManager.unityUIClicksAlwaysBlocks)
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && KickStarter.playerMenus.EventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
					{
						return true;
					}
					return false;
				}
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen || KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor || KickStarter.settingsManager.movementMethod == MovementMethod.Drag)
				{
					return KickStarter.playerMenus.EventSystem.IsPointerOverGameObject();
				}
			}
			return false;
		}

		protected void OnDestroy()
		{
			firstPersonCamera = null;
		}
	}
}
