using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera25_d.html")]
	public class GameCamera25D : _Camera
	{
		public BackgroundImage backgroundImage;

		public bool isActiveEditor;

		public Vector2 perspectiveOffset = Vector2.zero;

		public void SetActiveBackground()
		{
			if (!this.backgroundImage)
			{
				return;
			}
			if (!(BackgroundCamera.Instance != null) || BackgroundImageUI.Instance != null)
			{
			}
			BackgroundImage[] array = Object.FindObjectsOfType(typeof(BackgroundImage)) as BackgroundImage[];
			BackgroundImage[] array2 = array;
			foreach (BackgroundImage backgroundImage in array2)
			{
				if (backgroundImage == this.backgroundImage)
				{
					backgroundImage.TurnOn();
				}
				else
				{
					backgroundImage.TurnOff();
				}
			}
			KickStarter.mainCamera.PrepareForBackground();
		}

		public new void ResetTarget()
		{
		}

		public override Vector2 GetPerspectiveOffset()
		{
			return perspectiveOffset;
		}

		public override void MoveCameraInstant()
		{
			SetProjection();
		}

		protected void SetProjection()
		{
			if (MainCamera.AllowProjectionShifting(base.Camera))
			{
				base.Camera.projectionMatrix = AdvGame.SetVanishingPoint(base.Camera, perspectiveOffset);
			}
		}
	}
}
