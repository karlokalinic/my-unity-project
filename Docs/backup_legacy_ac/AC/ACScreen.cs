using UnityEngine;

namespace AC
{
	public static class ACScreen
	{
		public static int width
		{
			get
			{
				return Screen.width;
			}
		}

		public static int height
		{
			get
			{
				return Screen.height;
			}
		}

		public static Rect safeArea
		{
			get
			{
				return Screen.safeArea;
			}
		}
	}
}
