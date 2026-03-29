using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Misc/Parallax 2D")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_parallax2_d.html")]
	public class Parallax2D : MonoBehaviour
	{
		public float depth;

		public bool xScroll;

		public bool yScroll;

		public float xOffset;

		public float yOffset;

		public bool limitX;

		public float minX;

		public float maxX;

		public bool limitY;

		public float minY;

		public float maxY;

		public ParallaxReactsTo reactsTo;

		protected float xStart;

		protected float yStart;

		protected float xDesired;

		protected float yDesired;

		protected Vector2 perspectiveOffset;

		protected void Awake()
		{
			Initialise();
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public void UpdateOffset()
		{
			switch (reactsTo)
			{
			case ParallaxReactsTo.Camera:
				if (KickStarter.mainCamera.attachedCamera is GameCamera2D && !KickStarter.mainCamera.attachedCamera.Camera.orthographic)
				{
					perspectiveOffset = KickStarter.mainCamera.GetPerspectiveOffset();
				}
				else
				{
					perspectiveOffset = new Vector2(KickStarter.CameraMain.transform.position.x, KickStarter.CameraMain.transform.position.y);
				}
				break;
			case ParallaxReactsTo.Cursor:
			{
				Vector2 vector = ACScreen.safeArea.size / 2f;
				Vector2 mousePosition = KickStarter.playerInput.GetMousePosition();
				perspectiveOffset = new Vector2((1f - mousePosition.x) / vector.x + 1f, (1f - mousePosition.y) / vector.y + 1f);
				break;
			}
			}
			if (limitX)
			{
				perspectiveOffset.x = Mathf.Clamp(perspectiveOffset.x, minX, maxX);
			}
			if (limitY)
			{
				perspectiveOffset.y = Mathf.Clamp(perspectiveOffset.y, minY, maxY);
			}
			xDesired = xStart;
			if (xScroll)
			{
				xDesired += perspectiveOffset.x * depth;
				xDesired += xOffset;
			}
			yDesired = yStart;
			if (yScroll)
			{
				yDesired += perspectiveOffset.y * depth;
				yDesired += yOffset;
			}
			if (xScroll && yScroll)
			{
				base.transform.localPosition = new Vector3(xDesired, yDesired, base.transform.localPosition.z);
			}
			else if (xScroll)
			{
				base.transform.localPosition = new Vector3(xDesired, base.transform.localPosition.y, base.transform.localPosition.z);
			}
			else if (yScroll)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, yDesired, base.transform.localPosition.z);
			}
		}

		protected virtual void Initialise()
		{
			xStart = base.transform.localPosition.x;
			yStart = base.transform.localPosition.y;
			xDesired = xStart;
			yDesired = yStart;
		}
	}
}
