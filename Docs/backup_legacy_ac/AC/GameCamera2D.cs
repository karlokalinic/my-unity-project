using UnityEngine;

namespace AC
{
	[RequireComponent(typeof(Camera))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera2_d.html")]
	public class GameCamera2D : CursorInfluenceCamera
	{
		public bool lockHorizontal = true;

		public bool lockVertical = true;

		public bool limitHorizontal;

		public bool limitVertical;

		public Vector2 constrainHorizontal;

		public Vector2 constrainVertical;

		public Vector2 freedom = Vector2.zero;

		public float dampSpeed = 0.9f;

		public Vector2 directionInfluence = Vector2.zero;

		public Vector2 afterOffset = Vector2.zero;

		public bool doSnapping;

		public float unitSnap = 0.1f;

		protected Vector2 perspectiveOffset = Vector2.zero;

		protected Vector3 originalPosition = Vector3.zero;

		protected Vector2 desiredOffset = Vector2.zero;

		protected bool haveSetOriginalPosition;

		protected LerpUtils.FloatLerp xLerp = new LerpUtils.FloatLerp();

		protected LerpUtils.FloatLerp yLerp = new LerpUtils.FloatLerp();

		protected override void Awake()
		{
			SetOriginalPosition();
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			ResetTarget();
			if ((bool)target)
			{
				MoveCameraInstant();
			}
		}

		public override void _Update()
		{
			MoveCamera();
		}

		public override bool Is2D()
		{
			return true;
		}

		public override void MoveCameraInstant()
		{
			if (targetIsPlayer && (bool)KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
			SetOriginalPosition();
			if ((bool)target && (!lockHorizontal || !lockVertical))
			{
				SetDesired();
				if (!lockHorizontal)
				{
					perspectiveOffset.x = xLerp.Update(desiredOffset.x, desiredOffset.x, dampSpeed);
				}
				if (!lockVertical)
				{
					perspectiveOffset.y = yLerp.Update(desiredOffset.y, desiredOffset.y, dampSpeed);
				}
			}
			SetProjection();
		}

		public void SnapToOffset()
		{
			perspectiveOffset = afterOffset;
			SetProjection();
		}

		public void SetCorrectRotation()
		{
			if ((bool)KickStarter.settingsManager)
			{
				if (SceneSettings.IsTopDown())
				{
					base.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
					return;
				}
				if (SceneSettings.IsUnity2D())
				{
					base.Camera.orthographic = true;
				}
			}
			base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}

		public bool IsCorrectRotation()
		{
			if (SceneSettings.IsTopDown())
			{
				if (base.transform.rotation == Quaternion.Euler(90f, 0f, 0f))
				{
					return true;
				}
				return false;
			}
			if (SceneSettings.CameraPerspective != CameraPerspective.TwoD)
			{
				return true;
			}
			if (base.transform.rotation == Quaternion.Euler(0f, 0f, 0f))
			{
				return true;
			}
			return false;
		}

		public override Vector2 GetPerspectiveOffset()
		{
			return GetSnapOffset();
		}

		public void SetPerspectiveOffset(Vector2 _perspectiveOffset)
		{
			perspectiveOffset = _perspectiveOffset;
		}

		protected void SetDesired()
		{
			Vector2 offsetForPosition = GetOffsetForPosition(target.position);
			if (offsetForPosition.x < perspectiveOffset.x - freedom.x)
			{
				desiredOffset.x = offsetForPosition.x + freedom.x;
			}
			else if (offsetForPosition.x > perspectiveOffset.x + freedom.x)
			{
				desiredOffset.x = offsetForPosition.x - freedom.x;
			}
			desiredOffset.x += afterOffset.x;
			if (!Mathf.Approximately(directionInfluence.x, 0f))
			{
				desiredOffset.x += Vector3.Dot(base.TargetForward, base.transform.right) * directionInfluence.x;
			}
			if (limitHorizontal)
			{
				desiredOffset.x = ConstrainAxis(desiredOffset.x, constrainHorizontal);
			}
			if (offsetForPosition.y < perspectiveOffset.y - freedom.y)
			{
				desiredOffset.y = offsetForPosition.y + freedom.y;
			}
			else if (offsetForPosition.y > perspectiveOffset.y + freedom.y)
			{
				desiredOffset.y = offsetForPosition.y - freedom.y;
			}
			desiredOffset.y += afterOffset.y;
			if (!Mathf.Approximately(directionInfluence.y, 0f))
			{
				if (SceneSettings.IsTopDown())
				{
					desiredOffset.y += Vector3.Dot(base.TargetForward, base.transform.up) * directionInfluence.y;
				}
				else
				{
					desiredOffset.y += Vector3.Dot(base.TargetForward, base.transform.forward) * directionInfluence.y;
				}
			}
			if (limitVertical)
			{
				desiredOffset.y = ConstrainAxis(desiredOffset.y, constrainVertical);
			}
		}

		protected void MoveCamera()
		{
			if (targetIsPlayer && (bool)KickStarter.player)
			{
				target = KickStarter.player.transform;
			}
			if ((bool)target && (!lockHorizontal || !lockVertical))
			{
				SetDesired();
				if (!lockHorizontal)
				{
					perspectiveOffset.x = ((!(dampSpeed > 0f)) ? desiredOffset.x : xLerp.Update(perspectiveOffset.x, desiredOffset.x, dampSpeed));
				}
				if (!lockVertical)
				{
					perspectiveOffset.y = ((!(dampSpeed > 0f)) ? desiredOffset.y : yLerp.Update(perspectiveOffset.y, desiredOffset.y, dampSpeed));
				}
			}
			else if (!base.Camera.orthographic)
			{
				SnapToOffset();
			}
			SetProjection();
		}

		protected void SetOriginalPosition()
		{
			if (!haveSetOriginalPosition)
			{
				originalPosition = base.transform.position;
				haveSetOriginalPosition = true;
			}
		}

		protected void SetProjection()
		{
			Vector2 snapOffset = GetSnapOffset();
			if (base.Camera.orthographic)
			{
				base.transform.position = originalPosition + base.transform.right * snapOffset.x + base.transform.up * snapOffset.y;
			}
			else
			{
				base.Camera.projectionMatrix = AdvGame.SetVanishingPoint(base.Camera, snapOffset);
			}
		}

		protected Vector2 GetOffsetForPosition(Vector3 targetPosition)
		{
			Vector2 result = default(Vector2);
			float num = 93f - 299f * base.Camera.nearClipPlane;
			if (SceneSettings.IsTopDown())
			{
				if (base.Camera.orthographic)
				{
					result.x = base.transform.position.x;
					result.y = base.transform.position.z;
				}
				else
				{
					result.x = (0f - (targetPosition.x - base.transform.position.x)) / (num * (targetPosition.y - base.transform.position.y));
					result.y = (0f - (targetPosition.z - base.transform.position.z)) / (num * (targetPosition.y - base.transform.position.y));
				}
			}
			else
			{
				if (base.Camera.orthographic)
				{
					return base.transform.TransformVector(new Vector3(targetPosition.x, targetPosition.y, 0f - targetPosition.z));
				}
				float num2 = Vector3.Dot(base.transform.right, targetPosition - base.transform.position);
				float num3 = Vector3.Dot(base.transform.forward, targetPosition - base.transform.position);
				float num4 = Vector3.Dot(base.transform.up, targetPosition - base.transform.position);
				result.x = num2 / (num * num3);
				result.y = num4 / (num * num3);
			}
			return result;
		}

		protected Vector2 GetSnapOffset()
		{
			if (doSnapping)
			{
				Vector2 vector = perspectiveOffset;
				vector /= unitSnap;
				vector.x = Mathf.Round(vector.x);
				vector.y = Mathf.Round(vector.y);
				return vector * unitSnap;
			}
			return perspectiveOffset;
		}
	}
}
