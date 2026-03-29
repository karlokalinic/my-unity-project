using UnityEngine;

namespace AC
{
	public abstract class CursorInfluenceCamera : _Camera
	{
		public bool followCursor;

		public Vector2 cursorInfluence = new Vector2(0.3f, 0.1f);

		public bool constrainCursorInfluenceX;

		public Vector2 limitCursorInfluenceX;

		public bool constrainCursorInfluenceY;

		public Vector2 limitCursorInfluenceY;

		public float followCursorSpeed = 3f;

		protected Vector2 actualCursorOffset;

		public override Vector2 CreateRotationOffset()
		{
			if (followCursor)
			{
				if (KickStarter.stateHandler.IsInGameplay())
				{
					Vector2 mousePosition = KickStarter.playerInput.GetMousePosition();
					Vector2 vector = new Vector2(mousePosition.x / (float)(ACScreen.width / 2) - 1f, mousePosition.y / (float)(ACScreen.height / 2) - 1f);
					float sqrMagnitude = vector.sqrMagnitude;
					if (sqrMagnitude < 1.96f)
					{
						if (constrainCursorInfluenceX)
						{
							vector.x = Mathf.Clamp(vector.x, limitCursorInfluenceX[0], limitCursorInfluenceX[1]);
						}
						if (constrainCursorInfluenceY)
						{
							vector.y = Mathf.Clamp(vector.y, limitCursorInfluenceY[0], limitCursorInfluenceY[1]);
						}
					}
					actualCursorOffset = Vector2.Lerp(b: new Vector2(vector.x * cursorInfluence.x, vector.y * cursorInfluence.y), a: actualCursorOffset, t: Time.deltaTime * followCursorSpeed);
				}
				return actualCursorOffset;
			}
			return Vector2.zero;
		}
	}
}
