using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___curved.html")]
	public class DragTrack_Curved : DragTrack
	{
		public float maxAngle = 60f;

		public float radius = 2f;

		public bool doLoop;

		public bool generateColliders = true;

		protected Vector3 startPosition;

		public override bool Loops
		{
			get
			{
				return doLoop || maxAngle == 360f;
			}
		}

		public float MaxAngle
		{
			get
			{
				return (!Loops) ? maxAngle : 360f;
			}
		}

		public override bool UsesEndColliders
		{
			get
			{
				return !Loops && generateColliders;
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
				draggable.maxCollider = (Collider)Object.Instantiate(Resources.Load("DragCollider", typeof(Collider)));
			}
			if (draggable.minCollider == null)
			{
				draggable.minCollider = (Collider)Object.Instantiate(Resources.Load("DragCollider", typeof(Collider)));
			}
			float num = Mathf.Asin(draggable.ColliderWidth / radius) * 57.29578f;
			draggable.maxCollider.transform.position = startPosition;
			draggable.maxCollider.transform.up = -base.transform.up;
			draggable.maxCollider.transform.RotateAround(base.transform.position, base.transform.forward, maxAngle + num);
			draggable.minCollider.transform.position = startPosition;
			draggable.minCollider.transform.up = base.transform.up;
			draggable.minCollider.transform.RotateAround(base.transform.position, base.transform.forward, 0f - num);
			base.AssignColliders(draggable);
		}

		public override void Connect(Moveable_Drag draggable)
		{
			startPosition = base.transform.position + radius * base.transform.right;
			AssignColliders(draggable);
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
			float num = Vector3.Dot(force, draggable.transform.up);
			Vector3 force2 = draggable.transform.up * num;
			draggable.Rigidbody.AddForce(force2);
		}

		public override float GetScreenPointProportionAlong(Vector2 point)
		{
			Vector2 vector = KickStarter.CameraMain.WorldToScreenPoint(GetGizmoPosition(0f));
			Vector2 vector2 = KickStarter.CameraMain.WorldToScreenPoint(GetGizmoPosition(1f));
			Vector2 vector3 = KickStarter.CameraMain.WorldToScreenPoint(base.transform.position);
			Vector2 vector4 = vector - vector3;
			Vector2 to = vector2 - vector3;
			Vector2 to2 = point - vector3;
			float num = AdvGame.SignedAngle(vector4, to2);
			float num2 = AdvGame.SignedAngle(vector4, to);
			if (Vector3.Dot(base.transform.forward, KickStarter.CameraMain.transform.forward) < 0f)
			{
				num2 *= -1f;
				num *= -1f;
			}
			if (num2 < 0f)
			{
				num2 += 360f;
			}
			if (num < 0f)
			{
				num += 360f;
			}
			if (Loops)
			{
				num2 = 360f;
			}
			float num3 = 180f + num2 / 2f;
			if (num > num3)
			{
				num -= 360f;
			}
			return num / num2;
		}

		public override void SetPositionAlong(float proportionAlong, Moveable_Drag draggable)
		{
			Quaternion rotation = Quaternion.AngleAxis(proportionAlong * MaxAngle, base.transform.forward);
			draggable.transform.position = RotatePointAroundPivot(startPosition, base.transform.position, rotation);
			draggable.transform.rotation = Quaternion.AngleAxis(proportionAlong * MaxAngle, base.transform.forward) * base.transform.rotation;
			if (UsesEndColliders)
			{
				UpdateColliders(proportionAlong, draggable);
			}
			base.SetPositionAlong(proportionAlong, draggable);
		}

		public override float GetDecimalAlong(Moveable_Drag draggable)
		{
			float num = 360f - (360f - MaxAngle) / 2f;
			float num2 = Vector3.Angle(-base.transform.right, draggable.transform.position - base.transform.position);
			if (num2 < 180f && Vector3.Dot(draggable.transform.position - base.transform.position, base.transform.up) < 0f)
			{
				num2 *= -1f;
			}
			num2 = 180f - num2;
			if (!Loops && num2 > num)
			{
				return 0f;
			}
			return num2 / MaxAngle;
		}

		public override void SnapToTrack(Moveable_Drag draggable, bool onStart)
		{
			Vector3 direction = draggable.transform.InverseTransformDirection(draggable.Rigidbody.linearVelocity);
			direction.x = 0f;
			direction.z = 0f;
			draggable.Rigidbody.linearVelocity = draggable.transform.TransformDirection(direction);
			float num = Mathf.Clamp01(GetDecimalAlong(draggable));
			draggable.transform.rotation = Quaternion.AngleAxis(num * MaxAngle, base.transform.forward) * base.transform.rotation;
			draggable.transform.position = base.transform.position + draggable.transform.right * radius;
			if (onStart)
			{
				SetPositionAlong(num, draggable);
			}
		}

		public override void UpdateDraggable(Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong(draggable);
			SnapToTrack(draggable, false);
			if (UsesEndColliders)
			{
				UpdateColliders(draggable.trackValue, draggable);
			}
			if (!onlySnapOnPlayerRelease)
			{
				DoSnapCheck(draggable);
			}
		}

		public override Vector3 GetGizmoPosition(float proportionAlong)
		{
			Quaternion rotation = Quaternion.AngleAxis(proportionAlong * MaxAngle, base.transform.forward);
			Vector3 point = base.transform.position + radius * base.transform.right;
			return RotatePointAroundPivot(point, base.transform.position, rotation);
		}

		public override Vector3 GetForceToPosition(Moveable_Drag draggable, float targetProportionAlong)
		{
			float num = Mathf.Clamp01(targetProportionAlong) - draggable.trackValue;
			if (Loops)
			{
				if (num > 0.5f)
				{
					num -= 1f;
				}
				else if (num < -0.5f)
				{
					num += 1f;
				}
			}
			return draggable.transform.up * num * 1000f;
		}

		protected void UpdateColliders(float trackValue, Moveable_Drag draggable)
		{
			if (!(trackValue > 1f))
			{
				if (trackValue > 0.5f)
				{
					draggable.minCollider.enabled = false;
					draggable.maxCollider.enabled = true;
				}
				else
				{
					draggable.minCollider.enabled = true;
					draggable.maxCollider.enabled = false;
				}
			}
		}
	}
}
