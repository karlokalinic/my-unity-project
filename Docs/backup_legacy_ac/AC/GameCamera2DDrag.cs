using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera2_d_drag.html")]
	public class GameCamera2DDrag : _Camera
	{
		public RotationLock xLock;

		public RotationLock yLock;

		public float xSpeed = 5f;

		public float ySpeed = 5f;

		public float xAcceleration = 5f;

		public float xDeceleration = 5f;

		public float yAcceleration = 5f;

		public float yDeceleration = 5f;

		public bool invertX;

		public bool invertY;

		public float minX;

		public float maxX;

		public float minY;

		public float maxY;

		public float xOffset;

		public float yOffset;

		protected float deltaX;

		protected float deltaY;

		protected float xPos;

		protected float yPos;

		protected Vector2 perspectiveOffset;

		protected Vector3 originalPosition;

		protected bool _is2D;

		protected Vector2 lastMousePosition;

		protected Vector2 noInput = Vector2.zero;

		protected override void Awake()
		{
			isDragControlled = true;
			targetIsPlayer = false;
			SetOriginalPosition();
			if ((bool)KickStarter.settingsManager)
			{
				_is2D = SceneSettings.IsUnity2D();
			}
			base.Awake();
		}

		public override void _Update()
		{
			inputMovement = GetInputVector();
			if (xLock != RotationLock.Locked)
			{
				if (Mathf.Approximately(inputMovement.x, 0f))
				{
					deltaX = Mathf.Lerp(deltaX, 0f, xDeceleration * Time.deltaTime);
				}
				else
				{
					float num = Mathf.Abs(inputMovement.x) / 1000f;
					if (inputMovement.x > 0f)
					{
						deltaX = Mathf.Lerp(deltaX, xSpeed * num, xAcceleration * Time.deltaTime * inputMovement.x);
					}
					else if (inputMovement.x < 0f)
					{
						deltaX = Mathf.Lerp(deltaX, (0f - xSpeed) * num, xAcceleration * Time.deltaTime * (0f - inputMovement.x));
					}
				}
				if (xLock == RotationLock.Limited)
				{
					if ((invertX && deltaX > 0f) || (!invertX && deltaX < 0f))
					{
						if (maxX - xPos < 5f)
						{
							deltaX *= (maxX - xPos) / 5f;
						}
					}
					else if (((invertX && deltaX < 0f) || (!invertX && deltaX > 0f)) && minX - xPos > -5f)
					{
						deltaX *= (minX - xPos) / -5f;
					}
				}
				if (invertX)
				{
					xPos += deltaX / 100f;
				}
				else
				{
					xPos -= deltaX / 100f;
				}
				if (xLock == RotationLock.Limited)
				{
					xPos = Mathf.Clamp(xPos, minX, maxX);
				}
			}
			if (yLock != RotationLock.Locked)
			{
				if (Mathf.Approximately(inputMovement.y, 0f))
				{
					deltaY = Mathf.Lerp(deltaY, 0f, xDeceleration * Time.deltaTime);
				}
				else
				{
					float num2 = Mathf.Abs(inputMovement.y) / 1000f;
					if (inputMovement.y > 0f)
					{
						deltaY = Mathf.Lerp(deltaY, ySpeed * num2, xAcceleration * Time.deltaTime * inputMovement.y);
					}
					else if (inputMovement.y < 0f)
					{
						deltaY = Mathf.Lerp(deltaY, (0f - ySpeed) * num2, xAcceleration * Time.deltaTime * (0f - inputMovement.y));
					}
				}
				if (yLock == RotationLock.Limited)
				{
					if ((invertY && deltaY > 0f) || (!invertY && deltaY < 0f))
					{
						if (maxY - yPos < 5f)
						{
							deltaY *= (maxY - yPos) / 5f;
						}
					}
					else if (((invertY && deltaY < 0f) || (!invertY && deltaY > 0f)) && minY - yPos > -5f)
					{
						deltaY *= (minY - yPos) / -5f;
					}
				}
				if (invertY)
				{
					yPos += deltaY / 100f;
				}
				else
				{
					yPos -= deltaY / 100f;
				}
				if (yLock == RotationLock.Limited)
				{
					yPos = Mathf.Clamp(yPos, minY, maxY);
				}
			}
			if (xLock != RotationLock.Locked)
			{
				perspectiveOffset.x = xPos + xOffset;
			}
			if (yLock != RotationLock.Locked)
			{
				perspectiveOffset.y = yPos + yOffset;
			}
			SetProjection();
		}

		public override bool Is2D()
		{
			return _is2D;
		}

		public override Vector2 GetPerspectiveOffset()
		{
			return perspectiveOffset;
		}

		public void SetPosition(Vector2 _position)
		{
			xPos = _position.x;
			yPos = _position.y;
		}

		public Vector2 GetPosition()
		{
			return new Vector2(xPos, yPos);
		}

		protected virtual Vector2 GetInputVector()
		{
			if (KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != this)
			{
				return noInput;
			}
			if (KickStarter.stateHandler.gameState != GameState.Normal)
			{
				return noInput;
			}
			if (KickStarter.playerInput.GetDragState() == DragState._Camera)
			{
				return KickStarter.playerInput.GetDragVector();
			}
			return noInput;
		}

		protected void SetProjection()
		{
			if (!base.Camera.orthographic && _is2D)
			{
				base.Camera.projectionMatrix = AdvGame.SetVanishingPoint(base.Camera, perspectiveOffset);
			}
			else
			{
				base.transform.position = new Vector3(originalPosition.x + perspectiveOffset.x, originalPosition.y + perspectiveOffset.y, originalPosition.z);
			}
		}

		protected void SetOriginalPosition()
		{
			originalPosition = base.transform.position;
		}
	}
}
