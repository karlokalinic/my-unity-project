using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track.html")]
	public class DragTrack : MonoBehaviour
	{
		public PhysicsMaterial colliderMaterial;

		public float discSize = 0.2f;

		public Color handleColour = Color.white;

		public DragMovementCalculation dragMovementCalculation;

		public bool doSnapping;

		public List<TrackSnapData> allTrackSnapData = new List<TrackSnapData>();

		public float snapSpeed = 100f;

		public bool onlySnapOnPlayerRelease;

		public bool preventEndToEndJumping;

		public virtual bool Loops
		{
			get
			{
				return false;
			}
		}

		public virtual bool UsesEndColliders
		{
			get
			{
				return false;
			}
		}

		public virtual void AssignColliders(Moveable_Drag draggable)
		{
			if (UsesEndColliders && draggable.minCollider != null && draggable.maxCollider != null)
			{
				draggable.maxCollider.transform.rotation = Quaternion.AngleAxis(90f, draggable.maxCollider.transform.right) * draggable.maxCollider.transform.rotation;
				draggable.minCollider.transform.rotation = Quaternion.AngleAxis(90f, draggable.minCollider.transform.right) * draggable.minCollider.transform.rotation;
				if ((bool)colliderMaterial)
				{
					draggable.maxCollider.material = colliderMaterial;
					draggable.minCollider.material = colliderMaterial;
				}
				draggable.maxCollider.transform.parent = base.transform;
				draggable.minCollider.transform.parent = base.transform;
				draggable.maxCollider.name = draggable.name + "_UpperLimit";
				draggable.minCollider.name = draggable.name + "_LowerLimit";
			}
			LimitCollisions(draggable);
		}

		public virtual float GetDecimalAlong(Moveable_Drag draggable)
		{
			return 0f;
		}

		public virtual void SetPositionAlong(float proportionAlong, Moveable_Drag draggable)
		{
			draggable.trackValue = proportionAlong;
		}

		public virtual void Connect(Moveable_Drag draggable)
		{
		}

		public virtual void ApplyDragForce(Vector3 force, Moveable_Drag draggable)
		{
		}

		public virtual float GetScreenPointProportionAlong(Vector2 point)
		{
			return 0f;
		}

		public virtual void ApplyAutoForce(float _position, float _speed, Moveable_Drag draggable)
		{
		}

		public virtual void UpdateDraggable(Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong(draggable);
			if (!onlySnapOnPlayerRelease)
			{
				DoSnapCheck(draggable);
			}
		}

		public void OnLetGo(Moveable_Drag draggable)
		{
			DoSnapCheck(draggable);
		}

		public virtual void SnapToTrack(Moveable_Drag draggable, bool onStart)
		{
		}

		public virtual bool IconIsStationary()
		{
			return false;
		}

		public virtual Vector3 GetGizmoPosition(float proportionAlong)
		{
			return base.transform.position;
		}

		public virtual Vector3 GetForceToPosition(Moveable_Drag draggable, float targetProportionAlong)
		{
			return Vector3.zero;
		}

		public virtual float GetMoveSoundIntensity(Moveable_Drag draggable)
		{
			return draggable.Rigidbody.linearVelocity.magnitude;
		}

		public TrackSnapData GetSnapData(int ID)
		{
			foreach (TrackSnapData allTrackSnapDatum in allTrackSnapData)
			{
				if (allTrackSnapDatum.ID == ID)
				{
					return allTrackSnapDatum;
				}
			}
			return null;
		}

		public bool IsWithinSnapRegion(float trackValue, int snapID)
		{
			foreach (TrackSnapData allTrackSnapDatum in allTrackSnapData)
			{
				if (allTrackSnapDatum.ID == snapID)
				{
					return allTrackSnapDatum.IsWithinRegion(trackValue);
				}
			}
			return false;
		}

		protected void DoSnapCheck(Moveable_Drag draggable)
		{
			if (doSnapping && !draggable.IsAutoMoving() && !draggable.IsHeld)
			{
				SnapToNearest(draggable);
			}
		}

		protected void SnapToNearest(Moveable_Drag draggable)
		{
			int num = -1;
			float num2 = float.PositiveInfinity;
			for (int i = 0; i < allTrackSnapData.Count; i++)
			{
				float distanceFrom = allTrackSnapData[i].GetDistanceFrom(draggable.trackValue);
				if (distanceFrom < num2)
				{
					num = i;
					num2 = distanceFrom;
				}
			}
			if (num >= 0)
			{
				allTrackSnapData[num].MoveTo(draggable, snapSpeed);
			}
		}

		protected void LimitCollisions(Moveable_Drag draggable)
		{
			Collider[] array = Object.FindObjectsOfType(typeof(Collider)) as Collider[];
			Collider[] componentsInChildren = draggable.GetComponentsInChildren<Collider>();
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (collider.enabled)
					{
						if (collider != draggable.minCollider && draggable.minCollider.enabled)
						{
							Physics.IgnoreCollision(collider, draggable.minCollider, true);
						}
						if (collider != draggable.maxCollider && draggable.maxCollider.enabled)
						{
							Physics.IgnoreCollision(collider, draggable.maxCollider, true);
						}
					}
				}
			}
			Collider[] array3 = array;
			foreach (Collider collider2 in array3)
			{
				if (collider2.GetComponent<AC_Trigger>() != null)
				{
					continue;
				}
				Collider[] array4 = componentsInChildren;
				foreach (Collider collider3 in array4)
				{
					if (!(collider2 == collider3))
					{
						bool ignore = true;
						if ((draggable.minCollider != null && draggable.minCollider == collider2) || (draggable.maxCollider != null && draggable.maxCollider == collider2))
						{
							ignore = false;
						}
						else if (KickStarter.player != null && collider2.gameObject == KickStarter.player.gameObject)
						{
							ignore = draggable.ignorePlayerCollider;
						}
						else if ((bool)collider2.GetComponent<Rigidbody>() && collider2.gameObject != draggable.gameObject)
						{
							ignore = (bool)collider2.GetComponent<Moveable>() && draggable.ignoreMoveableRigidbodies;
						}
						if (collider2.enabled && collider3.enabled)
						{
							Physics.IgnoreCollision(collider2, collider3, ignore);
						}
					}
				}
			}
			if (!(draggable.minCollider != null) || !(draggable.maxCollider != null))
			{
				return;
			}
			Collider[] array5 = componentsInChildren;
			foreach (Collider collider4 in array5)
			{
				if (collider4.enabled && draggable.minCollider.enabled)
				{
					Physics.IgnoreCollision(collider4, draggable.minCollider, false);
				}
				if (collider4.enabled && draggable.maxCollider.enabled)
				{
					Physics.IgnoreCollision(collider4, draggable.maxCollider, false);
				}
			}
		}

		protected Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
		{
			return rotation * (point - pivot) + pivot;
		}
	}
}
