using UnityEngine;

namespace AC
{
	public class tk2DIntegration
	{
		public static bool IsDefinePresent()
		{
			return false;
		}

		public static bool PlayAnimation(Transform sprite, string clipName, int frame = -1)
		{
			ACDebug.Log("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
			return true;
		}

		public static bool Is2DtkSprite(GameObject spriteObject)
		{
			return false;
		}

		public static bool PlayAnimation(Transform sprite, string clipName, bool changeWrapMode, WrapMode wrapMode, int frame = -1)
		{
			ACDebug.Log("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
			return true;
		}

		public static void StopAnimation(Transform sprite)
		{
			ACDebug.Log("The 'tk2DIsPresent' preprocessor is not defined - check your Build Settings.");
		}

		public static bool IsAnimationPlaying(Transform sprite, string clipName)
		{
			return false;
		}
	}
}
