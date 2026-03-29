using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_moveable___drag.html")]
	public class Moveable_Drag : DragBase
	{
		protected class AutoMoveTrackData
		{
			protected float targetValue;

			protected float speed;

			protected LayerMask blockLayerMask;

			protected int snapID = -1;

			public AutoMoveTrackData(float _targetValue, float _speed, LayerMask _blockLayerMask, int _snapID = -1)
			{
				targetValue = _targetValue;
				speed = _speed;
				blockLayerMask = _blockLayerMask;
				snapID = _snapID;
			}

			public void Update(DragTrack track, Moveable_Drag draggable)
			{
				track.ApplyAutoForce(targetValue, speed, draggable);
			}

			public bool CheckForEnd(Moveable_Drag draggable, bool beAccurate = true)
			{
				float num = draggable.trackValue;
				if (draggable.track.Loops)
				{
					if (targetValue - num > 0.5f)
					{
						num += 1f;
					}
					else if (num - targetValue > 0.5f)
					{
						num -= 1f;
					}
				}
				float num2 = Mathf.Abs(targetValue - num);
				if (num2 < 0.01f)
				{
					if (draggable.canCallSnapEvents && snapID >= 0)
					{
						KickStarter.eventManager.Call_OnDraggableSnap(draggable, draggable.track, draggable.track.GetSnapData(snapID));
						draggable.canCallSnapEvents = false;
					}
					if (!beAccurate)
					{
						return true;
					}
				}
				if (num2 < 0.001f)
				{
					return true;
				}
				return false;
			}

			public void Stop(DragTrack track, Moveable_Drag draggable, bool snapToTarget)
			{
				if (snapToTarget)
				{
					track.SetPositionAlong(targetValue, draggable);
				}
			}

			public void ProcessCollision(Collision collision, Moveable_Drag draggable)
			{
				if ((blockLayerMask.value & (1 << collision.gameObject.layer)) != 0)
				{
					draggable.StopAutoMove(false);
				}
			}
		}

		protected class ChildTransformData
		{
			protected Vector3 originalPosition;

			protected Quaternion originalRotation;

			public ChildTransformData(Vector3 _originalPosition, Quaternion _originalRotation)
			{
				originalPosition = _originalPosition;
				originalRotation = _originalRotation;
			}

			public void UpdateTransform(Transform transform)
			{
				transform.position = originalPosition;
				transform.rotation = originalRotation;
			}
		}

		public DragMode dragMode;

		public DragTrack track;

		public bool retainOriginalTransform;

		public bool setOnStart = true;

		public float trackValueOnStart;

		public ActionListSource actionListSource;

		public Interaction interactionOnMove;

		public Interaction interactionOnDrop;

		public ActionListAsset actionListAssetOnMove;

		public ActionListAsset actionListAssetOnDrop;

		public int moveParameterID = -1;

		public int dropParameterID = -1;

		public AlignDragMovement alignMovement;

		public Transform plane;

		public bool noGravityWhenHeld = true;

		public bool moveWithRigidbody = true;

		protected Vector3 grabPositionRelative;

		protected float colliderRadius = 0.5f;

		protected float grabDistance = 0.5f;

		[HideInInspector]
		public float trackValue;

		[HideInInspector]
		public Vector3 _dragVector;

		[HideInInspector]
		public Collider maxCollider;

		[HideInInspector]
		public Collider minCollider;

		[HideInInspector]
		public int revolutions;

		protected bool canPlayCollideSound;

		protected float screenToWorldOffset;

		protected float lastFrameTotalCursorPositionAlong;

		protected bool endLocked;

		protected AutoMoveTrackData activeAutoMove;

		public bool canCallSnapEvents = true;

		private Vector3 thisFrameTorque;

		public float toruqeDamping = 2f;

		private LerpUtils.Vector3Lerp torqueDampingLerp = new LerpUtils.Vector3Lerp(true);

		public float ColliderWidth
		{
			get
			{
				return colliderRadius;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if ((bool)_rigidbody)
			{
				SetGravity(true);
				if (dragMode == DragMode.RotateOnly && (_rigidbody.constraints == RigidbodyConstraints.FreezeRotation || _rigidbody.constraints == RigidbodyConstraints.FreezeRotationX || _rigidbody.constraints == RigidbodyConstraints.FreezeRotationY || _rigidbody.constraints == RigidbodyConstraints.FreezeRotationZ))
				{
					ACDebug.LogWarning("Draggable " + base.gameObject.name + " has a Drag Mode of 'RotateOnly', but its rigidbody rotation is constrained. This may lead to inconsistent behaviour, and using a HingeTrack is advised instead.", base.gameObject);
				}
			}
			else if (dragMode == DragMode.RotateOnly && moveWithRigidbody)
			{
				moveWithRigidbody = false;
			}
			else
			{
				ACDebug.LogWarning("A Rigidbody is required on the Draggable object " + base.name, this);
			}
			SphereCollider component = GetComponent<SphereCollider>();
			if (component != null)
			{
				colliderRadius = component.radius * base.transform.localScale.x;
			}
			else if (dragMode == DragMode.LockToTrack && track != null && track.UsesEndColliders)
			{
				ACDebug.LogWarning("Cannot calculate collider radius for Draggable object '" + base.gameObject.name + "' - it should have either a SphereCollider attached, even if it's disabled.", this);
			}
			if (dragMode == DragMode.LockToTrack)
			{
				StartCoroutine(InitToTrack());
			}
		}

		public override void _FixedUpdate()
		{
			if (activeAutoMove != null)
			{
				activeAutoMove.Update(track, this);
			}
			else if (dragMode == DragMode.RotateOnly && !moveWithRigidbody)
			{
				base.transform.Rotate(thisFrameTorque, Space.World);
				thisFrameTorque = torqueDampingLerp.Update(thisFrameTorque, Vector3.zero, toruqeDamping);
			}
		}

		public float GetPositionAlong()
		{
			if (dragMode == DragMode.LockToTrack && (bool)track && track is DragTrack_Hinge)
			{
				return trackValue + (float)revolutions;
			}
			return trackValue;
		}

		public override void UpdateMovement()
		{
			base.UpdateMovement();
			if (dragMode == DragMode.LockToTrack && (bool)track)
			{
				track.UpdateDraggable(this);
				if (_rigidbody.angularVelocity != Vector3.zero || _rigidbody.linearVelocity != Vector3.zero)
				{
					RunInteraction(true);
				}
				if (IsAutoMoving() && activeAutoMove.CheckForEnd(this))
				{
					StopAutoMove();
				}
				if ((bool)collideSound && (bool)collideSoundClip && track is DragTrack_Hinge)
				{
					if (trackValue > 0.05f && trackValue < 0.95f)
					{
						canPlayCollideSound = true;
					}
					else if ((Mathf.Approximately(trackValue, 0f) || (!onlyPlayLowerCollisionSound && Mathf.Approximately(trackValue, 1f))) && canPlayCollideSound)
					{
						canPlayCollideSound = false;
						collideSound.Play(collideSoundClip, false);
					}
				}
			}
			else if (isHeld && dragMode == DragMode.RotateOnly && allowZooming && distanceToCamera > 0f)
			{
				LimitZoom();
			}
			if ((bool)moveSoundClip && (bool)moveSound)
			{
				if (dragMode == DragMode.LockToTrack && track != null)
				{
					PlayMoveSound(track.GetMoveSoundIntensity(this), trackValue);
				}
				else
				{
					PlayMoveSound(_rigidbody.linearVelocity.magnitude, trackValue);
				}
			}
		}

		public override void DrawGrabIcon()
		{
			if (isHeld && showIcon && KickStarter.CameraMain.WorldToScreenPoint(base.transform.position).z > 0f && icon != null)
			{
				if (dragMode == DragMode.LockToTrack && track != null && track.IconIsStationary())
				{
					Vector3 vector = KickStarter.CameraMain.WorldToScreenPoint(grabPositionRelative + base.transform.position);
					icon.Draw(new Vector3(vector.x, vector.y));
				}
				else
				{
					Vector3 vector2 = KickStarter.CameraMain.WorldToScreenPoint(grabPoint.position);
					icon.Draw(new Vector3(vector2.x, vector2.y));
				}
			}
		}

		public override void ApplyDragForce(Vector3 force, Vector3 mousePosition, float _distanceToCamera)
		{
			distanceToCamera = _distanceToCamera;
			force *= speedFactor * distanceToCamera / 50f;
			if (force.magnitude > maxSpeed)
			{
				force *= maxSpeed / force.magnitude;
			}
			if (dragMode == DragMode.LockToTrack)
			{
				if (!(track != null))
				{
					return;
				}
				switch (track.dragMovementCalculation)
				{
				case DragMovementCalculation.DragVector:
					track.ApplyDragForce(force, this);
					break;
				case DragMovementCalculation.CursorPosition:
				{
					float screenPointProportionAlong = track.GetScreenPointProportionAlong(mousePosition);
					float num = screenPointProportionAlong + screenToWorldOffset;
					if (track.preventEndToEndJumping)
					{
						bool flag = num >= 1f || num <= 0f;
						if (endLocked)
						{
							if (!flag)
							{
								endLocked = false;
							}
						}
						else if (flag)
						{
							endLocked = true;
						}
						if (track.Loops || !endLocked)
						{
							lastFrameTotalCursorPositionAlong = num;
						}
						else
						{
							num = lastFrameTotalCursorPositionAlong;
						}
					}
					track.ApplyAutoForce(num, speedFactor / 10f, this);
					break;
				}
				}
				return;
			}
			Vector3 vector = Vector3.Cross(force, KickStarter.CameraMain.transform.forward);
			if (dragMode == DragMode.MoveAlongPlane)
			{
				if (alignMovement == AlignDragMovement.AlignToPlane)
				{
					if ((bool)plane)
					{
						_rigidbody.AddForceAtPosition(Vector3.Cross(vector, plane.up), base.transform.position + plane.up * grabDistance);
					}
					else
					{
						ACDebug.LogWarning("No alignment plane assigned to " + base.name, this);
					}
				}
				else
				{
					_rigidbody.AddForceAtPosition(force, base.transform.position - KickStarter.CameraMain.transform.forward * grabDistance);
				}
			}
			else if (dragMode == DragMode.RotateOnly)
			{
				vector /= Mathf.Sqrt((grabPoint.position - base.transform.position).magnitude) * 2.4f * rotationFactor;
				if (moveWithRigidbody)
				{
					_rigidbody.AddTorque(vector);
				}
				else
				{
					thisFrameTorque = vector;
				}
				if (allowZooming)
				{
					UpdateZoom();
				}
			}
		}

		public override void LetGo(bool ignoreEvents = false)
		{
			canCallSnapEvents = true;
			SetGravity(true);
			if (dragMode == DragMode.RotateOnly && moveWithRigidbody)
			{
				_rigidbody.linearVelocity = Vector3.zero;
			}
			if (!ignoreEvents)
			{
				RunInteraction(false);
			}
			base.LetGo(ignoreEvents);
			if (dragMode == DragMode.LockToTrack && track != null)
			{
				track.OnLetGo(this);
			}
		}

		public override void Grab(Vector3 grabPosition)
		{
			isHeld = true;
			grabPoint.position = grabPosition;
			grabPositionRelative = grabPosition - base.transform.position;
			grabDistance = grabPositionRelative.magnitude;
			if (dragMode == DragMode.LockToTrack && track != null)
			{
				StopAutoMove(false);
				if (track.dragMovementCalculation == DragMovementCalculation.CursorPosition)
				{
					screenToWorldOffset = trackValue - track.GetScreenPointProportionAlong(KickStarter.playerInput.GetMousePosition());
					endLocked = false;
					if (track.Loops)
					{
						if (trackValue < 0.5f && screenToWorldOffset < -0.5f)
						{
							screenToWorldOffset += 1f;
						}
						else if (trackValue > 0.5f && screenToWorldOffset > 0.5f)
						{
							screenToWorldOffset -= 1f;
						}
					}
				}
				if (track is DragTrack_Straight)
				{
					UpdateScrewVector();
				}
				else if (track is DragTrack_Hinge)
				{
					_dragVector = grabPosition;
				}
			}
			SetGravity(false);
			if (dragMode == DragMode.RotateOnly && moveWithRigidbody)
			{
				_rigidbody.linearVelocity = Vector3.zero;
			}
		}

		public void UpdateScrewVector()
		{
			float num = Vector3.Dot(grabPoint.position - base.transform.position, base.transform.forward);
			float num2 = Vector3.Dot(grabPoint.position - base.transform.position, base.transform.right);
			_dragVector = base.transform.forward * (0f - num2) + base.transform.right * num;
		}

		public void StopAutoMove(bool snapToTarget = true)
		{
			if (IsAutoMoving())
			{
				activeAutoMove.Stop(track, this, snapToTarget);
				activeAutoMove = null;
				_rigidbody.linearVelocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}
		}

		public bool IsAutoMoving(bool beAccurate = true)
		{
			if (activeAutoMove != null)
			{
				if (!beAccurate && activeAutoMove.CheckForEnd(this, false))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public void AutoMoveAlongTrack(float _targetTrackValue, float _targetTrackSpeed, bool removePlayerControl)
		{
			AutoMoveAlongTrack(_targetTrackValue, _targetTrackSpeed, removePlayerControl, 1);
		}

		public void AutoMoveAlongTrack(float _targetTrackValue, float _targetTrackSpeed, bool removePlayerControl, LayerMask layerMask, int snapID = -1)
		{
			if (dragMode == DragMode.LockToTrack && track != null)
			{
				if (snapID < 0)
				{
					canCallSnapEvents = true;
				}
				if (_targetTrackSpeed <= 0f)
				{
					activeAutoMove = null;
					track.SetPositionAlong(_targetTrackValue, this);
					return;
				}
				if (removePlayerControl)
				{
					isHeld = false;
				}
				activeAutoMove = new AutoMoveTrackData(_targetTrackValue, _targetTrackSpeed / 6000f, layerMask, snapID);
			}
			else
			{
				ACDebug.LogWarning("Cannot move " + base.name + " along a track, because no track has been assigned to it", this);
			}
		}

		protected IEnumerator InitToTrack()
		{
			if (track != null)
			{
				ChildTransformData[] childTransformData = GetChildTransforms();
				track.Connect(this);
				if (retainOriginalTransform)
				{
					track.SnapToTrack(this, true);
					SetChildTransforms(childTransformData);
					yield return new WaitForEndOfFrame();
				}
				if (setOnStart)
				{
					track.SetPositionAlong(trackValueOnStart, this);
				}
				else
				{
					track.SnapToTrack(this, true);
				}
				trackValue = track.GetDecimalAlong(this);
			}
		}

		protected ChildTransformData[] GetChildTransforms()
		{
			List<ChildTransformData> list = new List<ChildTransformData>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				list.Add(new ChildTransformData(child.position, child.rotation));
			}
			return list.ToArray();
		}

		protected void SetChildTransforms(ChildTransformData[] childTransformData)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				childTransformData[i].UpdateTransform(child);
			}
		}

		protected void RunInteraction(bool onMove)
		{
			int num = ((!onMove) ? dropParameterID : moveParameterID);
			switch (actionListSource)
			{
			case ActionListSource.InScene:
			{
				Interaction interaction = ((!onMove) ? interactionOnDrop : interactionOnMove);
				if (!(interaction != null) || base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) || (onMove && KickStarter.actionListManager.IsListRunning(interaction)))
				{
					break;
				}
				if (num >= 0)
				{
					ActionParameter parameter2 = interaction.GetParameter(num);
					if (parameter2 != null && parameter2.parameterType == ParameterType.GameObject)
					{
						parameter2.gameObject = base.gameObject;
					}
				}
				interaction.Interact();
				break;
			}
			case ActionListSource.AssetFile:
			{
				ActionListAsset actionListAsset = ((!onMove) ? actionListAssetOnDrop : actionListAssetOnMove);
				if (!(actionListAsset != null) || base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) || (onMove && KickStarter.actionListAssetManager.IsListRunning(actionListAsset)))
				{
					break;
				}
				if (num >= 0)
				{
					ActionParameter parameter = actionListAsset.GetParameter(num);
					if (parameter != null && parameter.parameterType == ParameterType.GameObject)
					{
						parameter.gameObject = base.gameObject;
						if ((bool)GetComponent<ConstantID>())
						{
							parameter.intValue = GetComponent<ConstantID>().constantID;
						}
						else
						{
							ACDebug.LogWarning("Cannot set the value of parameter " + num + " ('" + parameter.label + "') as " + base.gameObject.name + " has no Constant ID component.", base.gameObject);
						}
					}
				}
				actionListAsset.Interact();
				break;
			}
			}
		}

		protected void OnCollisionEnter(Collision collision)
		{
			if (IsAutoMoving())
			{
				activeAutoMove.ProcessCollision(collision, this);
			}
			BaseOnCollisionEnter(collision);
		}

		protected void SetGravity(bool value)
		{
			if (dragMode != DragMode.LockToTrack && noGravityWhenHeld && _rigidbody != null)
			{
				_rigidbody.useGravity = value;
			}
		}
	}
}
