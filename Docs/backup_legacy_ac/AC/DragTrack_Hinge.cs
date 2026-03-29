using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___hinge.html")]
	public class DragTrack_Hinge : DragTrack
	{
		public float maxAngle = 60f;

		public float radius = 2f;

		public bool doLoop;

		public bool limitRevolutions;

		public int maxRevolutions;

		public bool alignDragToFront;

		public override bool Loops
		{
			get
			{
				return doLoop || maxAngle >= 360f;
			}
		}

		public float MaxAngle
		{
			get
			{
				return (!Loops) ? maxAngle : 360f;
			}
		}

		public override void Connect(Moveable_Drag draggable)
		{
			if (maxRevolutions < 1)
			{
				maxRevolutions = 1;
			}
			LimitCollisions(draggable);
		}

		public override void ApplyAutoForce(float _position, float _speed, Moveable_Drag draggable)
		{
			if (!(Time.time <= 0.2f))
			{
				Vector3 forceToPosition = GetForceToPosition(draggable, _position);
				forceToPosition *= _speed / draggable.Rigidbody.mass;
				if (forceToPosition.magnitude > draggable.maxSpeed)
				{
					forceToPosition *= draggable.maxSpeed / forceToPosition.magnitude;
				}
				forceToPosition -= draggable.Rigidbody.angularVelocity;
				draggable.Rigidbody.AddTorque(forceToPosition, ForceMode.VelocityChange);
			}
		}

		public override void ApplyDragForce(Vector3 force, Moveable_Drag draggable)
		{
			float num = 0f;
			Vector3 vector = Vector2.zero;
			if (alignDragToFront)
			{
				num = Vector3.Dot(force, base.transform.up);
				vector = GetAxisOffset(draggable._dragVector);
				if (Vector3.Dot(base.transform.right, vector) < 0f)
				{
					num *= -1f;
				}
			}
			else
			{
				num = Vector3.Dot(force, draggable.transform.up);
				vector = GetAxisOffset(draggable.GetGrabPosition());
				if (Vector3.Dot(draggable.transform.right, vector) < 0f)
				{
					num *= -1f;
				}
			}
			Vector3 normalized = (draggable.transform.forward * num).normalized;
			normalized *= force.magnitude;
			normalized /= vector.magnitude / 0.43f;
			draggable.Rigidbody.AddTorque(normalized);
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
			draggable.transform.position = base.transform.position;
			draggable.transform.rotation = Quaternion.AngleAxis(proportionAlong * MaxAngle, base.transform.forward) * base.transform.rotation;
			base.SetPositionAlong(proportionAlong, draggable);
		}

		public override float GetDecimalAlong(Moveable_Drag draggable)
		{
			float num = Vector3.Angle(base.transform.up, draggable.transform.up);
			if (Vector3.Dot(-base.transform.right, draggable.transform.up) < 0f)
			{
				num = 360f - num;
			}
			if (num > 180f + MaxAngle / 2f)
			{
				num = 0f;
			}
			return num / MaxAngle;
		}

		public override void SnapToTrack(Moveable_Drag draggable, bool onStart)
		{
			draggable.transform.position = base.transform.position;
			if (onStart)
			{
				draggable.transform.rotation = base.transform.rotation;
				draggable.trackValue = 0f;
			}
		}

		public override void UpdateDraggable(Moveable_Drag draggable)
		{
			float trackValue = draggable.trackValue;
			draggable.transform.position = base.transform.position;
			draggable.trackValue = GetDecimalAlong(draggable);
			if (draggable.trackValue <= 0f || draggable.trackValue > 1f)
			{
				if (draggable.trackValue < 0f)
				{
					draggable.trackValue = 0f;
				}
				else if (draggable.trackValue > 1f)
				{
					draggable.trackValue = 1f;
				}
				draggable.Rigidbody.angularVelocity = Vector3.zero;
			}
			SetPositionAlong(draggable.trackValue, draggable);
			if (Loops && limitRevolutions)
			{
				if (trackValue < 0.1f && draggable.trackValue > 0.9f)
				{
					draggable.revolutions--;
				}
				else if (trackValue > 0.9f && draggable.trackValue < 0.1f)
				{
					draggable.revolutions++;
				}
				if (draggable.revolutions < 0)
				{
					draggable.revolutions = 0;
					draggable.trackValue = 0f;
					SetPositionAlong(draggable.trackValue, draggable);
					draggable.Rigidbody.angularVelocity = Vector3.zero;
				}
				else if (draggable.revolutions > maxRevolutions - 1)
				{
					draggable.revolutions = maxRevolutions - 1;
					draggable.trackValue = 1f;
					SetPositionAlong(draggable.trackValue, draggable);
					draggable.Rigidbody.angularVelocity = Vector3.zero;
				}
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
			return draggable.transform.forward * num * 1000f;
		}

		public override float GetMoveSoundIntensity(Moveable_Drag draggable)
		{
			return draggable.Rigidbody.angularVelocity.magnitude;
		}

		protected Vector3 GetAxisOffset(Vector3 grabPosition)
		{
			float num = Vector3.Dot(grabPosition, base.transform.forward);
			Vector3 vector = base.transform.position + base.transform.forward * num;
			return grabPosition - vector;
		}
	}
}
