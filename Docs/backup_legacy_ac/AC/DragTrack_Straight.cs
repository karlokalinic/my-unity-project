using System;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___straight.html")]
	public class DragTrack_Straight : DragTrack
	{
		public DragRotationType rotationType;

		public float maxDistance = 2f;

		public bool dragMustScrew;

		public float screwThread = 1f;

		public bool generateColliders = true;

		public override bool UsesEndColliders
		{
			get
			{
				return generateColliders;
			}
		}

		public override void AssignColliders(Moveable_Drag draggable)
		{
			if (!UsesEndColliders)
			{
				base.AssignColliders(draggable);
				return;
			}
			if (draggable.maxCollider == null)
			{
				draggable.maxCollider = (Collider)UnityEngine.Object.Instantiate(Resources.Load("DragCollider", typeof(Collider)));
			}
			if (draggable.minCollider == null)
			{
				draggable.minCollider = (Collider)UnityEngine.Object.Instantiate(Resources.Load("DragCollider", typeof(Collider)));
			}
			draggable.maxCollider.transform.position = base.transform.position + base.transform.up * maxDistance + base.transform.up * draggable.ColliderWidth;
			draggable.minCollider.transform.position = base.transform.position - base.transform.up * draggable.ColliderWidth;
			draggable.minCollider.transform.up = base.transform.up;
			draggable.maxCollider.transform.up = -base.transform.up;
			base.AssignColliders(draggable);
		}

		public override void Connect(Moveable_Drag draggable)
		{
			AssignColliders(draggable);
		}

		public override float GetDecimalAlong(Moveable_Drag draggable)
		{
			return (draggable.transform.position - base.transform.position).magnitude / maxDistance;
		}

		public override void SetPositionAlong(float proportionAlong, Moveable_Drag draggable)
		{
			draggable.transform.position = base.transform.position + base.transform.up * proportionAlong * maxDistance;
			if (rotationType != DragRotationType.None)
			{
				SetRotation(draggable, proportionAlong);
			}
			base.SetPositionAlong(proportionAlong, draggable);
		}

		public override void SnapToTrack(Moveable_Drag draggable, bool onStart)
		{
			Vector3 lhs = draggable.transform.position - base.transform.position;
			float value = Vector3.Dot(lhs, base.transform.up) / maxDistance;
			value = Mathf.Clamp01(value);
			if (onStart)
			{
				if (rotationType != DragRotationType.None)
				{
					SetRotation(draggable, value);
				}
				Rigidbody rigidbody = draggable.Rigidbody;
				Vector3 zero = Vector3.zero;
				draggable.Rigidbody.angularVelocity = zero;
				rigidbody.linearVelocity = zero;
			}
			draggable.transform.position = base.transform.position + base.transform.up * value * maxDistance;
			Vector3 direction = base.transform.InverseTransformDirection(draggable.Rigidbody.linearVelocity);
			direction.x = 0f;
			direction.z = 0f;
			draggable.Rigidbody.linearVelocity = base.transform.TransformDirection(direction);
		}

		public override void ApplyAutoForce(float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 forceToPosition = GetForceToPosition(draggable, _position);
			forceToPosition *= _speed / draggable.Rigidbody.mass;
			if (forceToPosition.magnitude > draggable.maxSpeed)
			{
				forceToPosition *= draggable.maxSpeed / forceToPosition.magnitude;
			}
			forceToPosition -= draggable.Rigidbody.linearVelocity;
			draggable.Rigidbody.AddForce(forceToPosition, ForceMode.VelocityChange);
		}

		public override void ApplyDragForce(Vector3 force, Moveable_Drag draggable)
		{
			float num = 0f;
			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					draggable.UpdateScrewVector();
				}
				num = Vector3.Dot(force, draggable._dragVector);
			}
			else
			{
				num = Vector3.Dot(force, base.transform.up);
			}
			Vector3 force2 = base.transform.up * num;
			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					force2 = (base.transform.up * num).normalized * force.magnitude;
					force2 /= Mathf.Sqrt((draggable.GetGrabPosition() - draggable.transform.position).magnitude) / 0.4f;
				}
				force2 /= Mathf.Sqrt(screwThread);
			}
			draggable.Rigidbody.AddForce(force2, ForceMode.Force);
		}

		public override float GetScreenPointProportionAlong(Vector2 point)
		{
			Vector3 position = base.transform.position + base.transform.up * maxDistance;
			Vector2 vector = KickStarter.CameraMain.WorldToScreenPoint(base.transform.position);
			Vector2 vector2 = KickStarter.CameraMain.WorldToScreenPoint(position);
			Vector2 vector3 = vector - vector2;
			Vector2 to = point - vector2;
			float num = Vector2.Angle(vector3, to);
			return 1f - Mathf.Cos(num * ((float)Math.PI / 180f)) * to.magnitude / vector3.magnitude;
		}

		public override bool IconIsStationary()
		{
			if (dragMovementCalculation == DragMovementCalculation.CursorPosition)
			{
				return true;
			}
			if (rotationType == DragRotationType.Roll || (rotationType == DragRotationType.Screw && !dragMustScrew))
			{
				return true;
			}
			return false;
		}

		public override void UpdateDraggable(Moveable_Drag draggable)
		{
			SnapToTrack(draggable, false);
			draggable.trackValue = GetDecimalAlong(draggable);
			if (rotationType != DragRotationType.None)
			{
				SetRotation(draggable, draggable.trackValue);
			}
			if (!onlySnapOnPlayerRelease)
			{
				DoSnapCheck(draggable);
			}
		}

		public override Vector3 GetGizmoPosition(float proportionAlong)
		{
			return base.transform.position + base.transform.up * proportionAlong * maxDistance;
		}

		public override Vector3 GetForceToPosition(Moveable_Drag draggable, float targetProportionAlong)
		{
			float num = Mathf.Clamp01(targetProportionAlong) - draggable.trackValue;
			return base.transform.up * num * 1000f;
		}

		protected void SetRotation(Moveable_Drag draggable, float proportionAlong)
		{
			float num = proportionAlong * maxDistance / draggable.ColliderWidth / 2f * 57.29578f;
			if (rotationType == DragRotationType.Roll)
			{
				draggable.Rigidbody.rotation = Quaternion.AngleAxis(num, base.transform.forward) * base.transform.rotation;
			}
			else if (rotationType == DragRotationType.Screw)
			{
				draggable.Rigidbody.rotation = Quaternion.AngleAxis(num * screwThread, base.transform.up) * base.transform.rotation;
			}
		}
	}
}
