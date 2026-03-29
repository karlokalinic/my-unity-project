using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Camera/First-person camera")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_first_person_camera.html")]
	public class FirstPersonCamera : _Camera
	{
		public Vector2 sensitivity = new Vector2(15f, 15f);

		public float minY = -60f;

		public float maxY = 60f;

		public bool allowMouseWheelZooming;

		public float minimumZoom = 13f;

		public float maximumZoom = 65f;

		public bool headBob = true;

		public FirstPersonHeadBobMethod headBobMethod;

		public float builtInSpeedFactor = 1f;

		public float bobbingAmount = 0.2f;

		private Animator headBobAnimator;

		public string headBobSpeedParameter;

		protected float actualTilt;

		protected float bobTimer;

		protected float height;

		protected float deltaHeight;

		protected Player player;

		protected LerpUtils.FloatLerp tiltLerp = new LerpUtils.FloatLerp();

		protected float targetTilt;

		protected override void Awake()
		{
			height = base.transform.localPosition.y;
			player = GetComponentInParent<Player>();
			headBobAnimator = GetComponent<Animator>();
		}

		public void AfterLoad()
		{
			Awake();
		}

		public new void ResetTarget()
		{
		}

		public void _UpdateFPCamera()
		{
			if (actualTilt != targetTilt)
			{
				if (player != null)
				{
					actualTilt = tiltLerp.Update(actualTilt, targetTilt, player.turnSpeed);
				}
				else
				{
					actualTilt = tiltLerp.Update(actualTilt, targetTilt, 7f);
				}
			}
			ApplyTilt();
			if (headBob)
			{
				if (headBobMethod == FirstPersonHeadBobMethod.BuiltIn)
				{
					deltaHeight = 0f;
					float headBobSpeed = GetHeadBobSpeed();
					float num = Mathf.Sin(bobTimer);
					bobTimer += Mathf.Abs(player.GetMoveSpeed()) * Time.deltaTime * 5f * builtInSpeedFactor;
					if (bobTimer > (float)Math.PI * 2f)
					{
						bobTimer -= (float)Math.PI * 2f;
					}
					float num2 = Mathf.Clamp(headBobSpeed, 0f, 1f);
					deltaHeight = num2 * num * bobbingAmount;
					base.transform.localPosition = new Vector3(base.transform.localPosition.x, height + deltaHeight, base.transform.localPosition.z);
				}
				else if (headBobMethod == FirstPersonHeadBobMethod.CustomAnimation && headBobAnimator != null && headBobSpeedParameter != string.Empty)
				{
					headBobAnimator.SetFloat(headBobSpeedParameter, GetHeadBobSpeed());
				}
			}
			if (KickStarter.stateHandler.gameState == GameState.Normal && allowMouseWheelZooming && base.Camera != null && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				float num3 = KickStarter.playerInput.InputGetAxis("Mouse ScrollWheel");
				if (num3 > 0f)
				{
					base.Camera.fieldOfView = Mathf.Max(base.Camera.fieldOfView - 3f, minimumZoom);
				}
				else if (num3 < 0f)
				{
					base.Camera.fieldOfView = Mathf.Min(base.Camera.fieldOfView + 3f, maximumZoom);
				}
			}
		}

		public float GetHeadBobSpeed()
		{
			if (player != null && player.IsGrounded(true))
			{
				return Mathf.Abs(player.GetMoveSpeed());
			}
			return 0f;
		}

		public void SetPitch(float angle, bool isInstant = true)
		{
			if (isInstant)
			{
				actualTilt = (targetTilt = angle);
			}
			else
			{
				targetTilt = angle;
			}
		}

		public void IncreasePitch(float increase)
		{
			actualTilt += increase * sensitivity.y * Time.deltaTime * 50f;
			targetTilt = actualTilt;
		}

		public bool IsTilting()
		{
			return actualTilt != 0f;
		}

		public float GetTilt()
		{
			return actualTilt;
		}

		protected void ApplyTilt()
		{
			actualTilt = Mathf.Clamp(actualTilt, minY, maxY);
			base.transform.localEulerAngles = new Vector3(actualTilt, 0f, 0f);
		}
	}
}
