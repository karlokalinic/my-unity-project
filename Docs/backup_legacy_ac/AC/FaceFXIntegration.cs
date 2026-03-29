using UnityEngine;

namespace AC
{
	public class FaceFXIntegration
	{
		public static bool IsDefinePresent()
		{
			return false;
		}

		public static void Play(Char speaker, string name, AudioClip audioClip)
		{
			ACDebug.LogWarning("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.");
		}

		public static void Stop(Char speaker)
		{
			ACDebug.LogWarning("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.");
		}
	}
}
